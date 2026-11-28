using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Interfaces
{
    /// <summary>
    /// One detected object, coordinates in pixels of the source image
    /// </summary>
    public class YoloDetection
    {
        public string Label;
        public float Confidence;
        public RectangleF Box;
    }

    /// <summary>
    /// YOLOv8 ONNX model running in-process with ONNX Runtime (replaces the Python YOLO server).
    /// One shared session for all cameras: InferenceSession.Run is thread-safe.
    /// </summary>
    public sealed class YoloModel : IDisposable
    {
        public const string DefaultModelFile = "yolov8n.onnx";

        const int InputSize = 640;
        const float ConfidenceThreshold = 0.25f;
        const float NmsIouThreshold = 0.45f;

        static readonly object sharedLock = new object();
        static YoloModel shared;

        /// <summary>
        /// Model from &lt;application folder&gt;\models\yolov8n.onnx, loaded once on first use
        /// </summary>
        public static YoloModel Shared
        {
            get
            {
                lock (sharedLock)
                {
                    if (shared == null)
                    {
                        string dir = Path.GetDirectoryName(typeof(YoloModel).Assembly.Location);
                        shared = new YoloModel(Path.Combine(dir, "models", DefaultModelFile));
                    }
                    return shared;
                }
            }
        }

        readonly InferenceSession session;
        readonly string inputName;
        readonly string[] classNames;

        public YoloModel(string modelPath)
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException("YOLO model not found", modelPath);

            var options = new SessionOptions();
            // leave CPU for video decoding: detection runs in background, speed is not critical
            options.IntraOpNumThreads = Math.Max(1, Environment.ProcessorCount / 2);
            session = new InferenceSession(modelPath, options);
            inputName = session.InputMetadata.Keys.First();
            classNames = ReadClassNames(session) ?? CocoClassNames;
        }

        /// <summary>
        /// Ultralytics stores class names in model metadata as "{0: 'person', 1: 'bicycle', ...}"
        /// </summary>
        static string[] ReadClassNames(InferenceSession session)
        {
            string names;
            if (!session.ModelMetadata.CustomMetadataMap.TryGetValue("names", out names))
                return null;

            var map = new SortedDictionary<int, string>();
            foreach (Match m in Regex.Matches(names, @"(\d+)\s*:\s*'([^']*)'"))
                map[int.Parse(m.Groups[1].Value)] = m.Groups[2].Value;
            return map.Count > 0 ? map.Values.ToArray() : null;
        }

        public List<YoloDetection> Detect(Bitmap image)
        {
            // letterbox: keep aspect ratio, pad with gray (114) like Ultralytics
            float scale = Math.Min((float)InputSize / image.Width, (float)InputSize / image.Height);
            int w = (int)Math.Round(image.Width * scale);
            int h = (int)Math.Round(image.Height * scale);
            int padX = (InputSize - w) / 2;
            int padY = (InputSize - h) / 2;

            var input = new DenseTensor<float>(new[] { 1, 3, InputSize, InputSize });
            using (var letterbox = new Bitmap(InputSize, InputSize, PixelFormat.Format24bppRgb))
            {
                using (Graphics g = Graphics.FromImage(letterbox))
                {
                    g.Clear(Color.FromArgb(114, 114, 114));
                    g.InterpolationMode = InterpolationMode.Bilinear;
                    g.DrawImage(image, padX, padY, w, h);
                }
                FillTensor(letterbox, input);
            }

            var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor(inputName, input) };
            using (var results = session.Run(inputs))
            {
                // YOLOv8 output: [1, 4 + classes, anchors] = cx, cy, w, h, class scores...
                Tensor<float> output = results.First().AsTensor<float>();
                int channels = output.Dimensions[1];
                int anchors = output.Dimensions[2];
                int classes = channels - 4;

                var candidates = new List<YoloDetection>();
                for (int a = 0; a < anchors; a++)
                {
                    int best = -1;
                    float bestScore = ConfidenceThreshold;
                    for (int c = 0; c < classes; c++)
                    {
                        float score = output[0, 4 + c, a];
                        if (score > bestScore)
                        {
                            bestScore = score;
                            best = c;
                        }
                    }
                    if (best < 0)
                        continue;

                    float cx = output[0, 0, a], cy = output[0, 1, a];
                    float bw = output[0, 2, a], bh = output[0, 3, a];

                    // undo letterbox -> source image pixels
                    float x1 = (cx - bw / 2 - padX) / scale;
                    float y1 = (cy - bh / 2 - padY) / scale;
                    float x2 = (cx + bw / 2 - padX) / scale;
                    float y2 = (cy + bh / 2 - padY) / scale;
                    x1 = Clamp(x1, 0, image.Width); x2 = Clamp(x2, 0, image.Width);
                    y1 = Clamp(y1, 0, image.Height); y2 = Clamp(y2, 0, image.Height);

                    candidates.Add(new YoloDetection
                    {
                        Label = best < classNames.Length ? classNames[best] : best.ToString(),
                        Confidence = bestScore,
                        Box = RectangleF.FromLTRB(x1, y1, x2, y2)
                    });
                }
                return Nms(candidates);
            }
        }

        static unsafe void FillTensor(Bitmap bmp, DenseTensor<float> tensor)
        {
            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                Span<float> t = tensor.Buffer.Span;
                int plane = InputSize * InputSize;
                for (int y = 0; y < InputSize; y++)
                {
                    byte* row = (byte*)data.Scan0 + y * data.Stride;
                    for (int x = 0; x < InputSize; x++)
                    {
                        int i = y * InputSize + x;
                        byte* px = row + x * 3;           // BGR
                        t[i] = px[2] / 255f;              // R
                        t[plane + i] = px[1] / 255f;      // G
                        t[2 * plane + i] = px[0] / 255f;  // B
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }
        }

        /// <summary>
        /// Non-maximum suppression per class
        /// </summary>
        static List<YoloDetection> Nms(List<YoloDetection> candidates)
        {
            var result = new List<YoloDetection>();
            foreach (var group in candidates.GroupBy(d => d.Label))
            {
                var sorted = group.OrderByDescending(d => d.Confidence).ToList();
                while (sorted.Count > 0)
                {
                    var top = sorted[0];
                    result.Add(top);
                    sorted.RemoveAt(0);
                    sorted.RemoveAll(d => IoU(top.Box, d.Box) > NmsIouThreshold);
                }
            }
            return result;
        }

        static float IoU(RectangleF a, RectangleF b)
        {
            RectangleF i = RectangleF.Intersect(a, b);
            if (i.IsEmpty)
                return 0;
            float inter = i.Width * i.Height;
            float union = a.Width * a.Height + b.Width * b.Height - inter;
            return union <= 0 ? 0 : inter / union;
        }

        static float Clamp(float v, float min, float max)
        {
            return v < min ? min : (v > max ? max : v);
        }

        public void Dispose()
        {
            session.Dispose();
        }

        static readonly string[] CocoClassNames =
        {
            "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light",
            "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow",
            "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee",
            "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard",
            "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple",
            "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "couch",
            "potted plant", "bed", "dining table", "toilet", "tv", "laptop", "mouse", "remote", "keyboard",
            "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase",
            "scissors", "teddy bear", "hair drier", "toothbrush"
        };
    }
}
