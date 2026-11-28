using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace Interfaces
{
    /// <summary>
    /// Object detection for one camera. Frames are processed on a background thread with the
    /// in-process YOLO model (no Python server). While a frame is being processed new frames are skipped.
    /// </summary>
    public class DetectorYOLO
    {
        // names used in camera settings -> YOLO (COCO) labels; keeps old settings working
        static readonly Dictionary<string, string> LabelAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "human", "person" },
            { "bicecle", "bicycle" },
        };

        static int modelErrorShown = 0;

        readonly AutoResetEvent frameReady = new AutoResetEvent(false);
        readonly Thread thread;
        readonly object sync = new object();

        int busy = 0;               // 1 while a frame is waiting or being processed
        Bitmap frame;               // own copy of the frame to process
        string path = "";
        string fileName = "";
        string[] obj2detect = new string[0];

        public DetectorYOLO(string cameraName)
        {
            thread = new Thread(DetectRequestTask);
            thread.Name = String.Format("YOLO[{0}]", cameraName);
            thread.IsBackground = true;
            thread.Start();
        }

        private void DetectRequestTask()
        {
            while (true)
            {
                frameReady.WaitOne();

                Bitmap image;
                string savePath, saveName;
                string[] targets;
                lock (sync)
                {
                    image = frame;
                    frame = null;
                    savePath = path;
                    saveName = fileName;
                    targets = obj2detect;
                }

                try
                {
                    if (image != null && targets.Length > 0)
                        Detect(image, savePath, saveName, targets);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    image?.Dispose();
                    Interlocked.Exchange(ref busy, 0);
                }
            }
        }

        private static void Detect(Bitmap image, string savePath, string saveName, string[] targets)
        {
            YoloModel model;
            try
            {
                model = YoloModel.Shared;
            }
            catch (Exception ex)
            {
                // report a missing/broken model once, not for every frame of every camera
                if (Interlocked.Exchange(ref modelErrorShown, 1) == 0)
                    ThreadPool.QueueUserWorkItem(_ => System.Windows.Forms.MessageBox.Show(
                        "Не вдалося завантажити модель YOLO.\n\n" + ex.Message, "YOLO",
                        System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning));
                throw;
            }

            List<YoloDetection> found = model.Detect(image)
                .Where(d => targets.Any(t => string.Equals(t, d.Label, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            if (found.Count == 0)
                return;

            using (Graphics g = Graphics.FromImage(image))
            using (Pen pen = new Pen(Color.Red, 1))
            using (Font font = new Font("Arial", 10))
            using (SolidBrush brush = new SolidBrush(Color.Yellow))
            {
                foreach (YoloDetection d in found)
                {
                    Console.WriteLine($"Found {d.Label} with confidence {d.Confidence}");
                    g.DrawRectangle(pen, d.Box.X, d.Box.Y, d.Box.Width, d.Box.Height);
                    g.DrawString($"{d.Label} ({d.Confidence:P1})", font, brush, new PointF(d.Box.X, d.Box.Y - 20));
                }
            }

            string filePath = Path.Combine(savePath, $"{saveName}.jpg");
            image.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
        }

        /// <summary>
        /// Queue a frame for detection; ignored while the previous frame is still processed.
        /// </summary>
        public void DetectObjects(Bitmap bitmap, string path, string fileName, string[] obj2detect)
        {
            if (bitmap == null || Interlocked.CompareExchange(ref busy, 1, 0) != 0)
                return;

            Bitmap copy;
            try
            {
                // the caller keeps drawing new frames into its bitmap - work on our own copy
                copy = new Bitmap(bitmap);
            }
            catch
            {
                Interlocked.Exchange(ref busy, 0);
                throw;
            }

            lock (sync)
            {
                frame = copy;
                this.path = path;
                this.fileName = fileName;
                this.obj2detect = (obj2detect ?? new string[0])
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .Select(o => LabelAliases.TryGetValue(o.Trim(), out string label) ? label : o.Trim())
                    .ToArray();
            }
            frameReady.Set();
        }
    }
}
