using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using LibVLCSharp.Shared;

namespace Interfaces
{
    internal class CameraM3U8 : SourceFormater
    {
        private static readonly LibVLC SharedLibVLC;
        private int _restart_time = 5000;
        static CameraM3U8()
        {
            Core.Initialize();
            SharedLibVLC = new LibVLC();
#if DEBUG
            SharedLibVLC.Log += (sender, e) => Console.WriteLine($"[{e.Level}] {e.Module}:{e.Message}");
#endif
        }

        private const uint width = 640;
        private const uint height = 480;
        private const uint pitch = width * 4;
        private const int bufferSize = (int)(width * height * 4);

        public override event CameraEventHandler NewFrame;
        Bitmap yoloCopy;

        /// <summary>
        /// State of one Start() run. Each run has its own buffer and stop signal, so Stop() does not
        /// have to wait for the worker: an old worker finishing late can't touch a newer run's data.
        /// </summary>
        private sealed class Run
        {
            public readonly ManualResetEvent Stop = new ManualResetEvent(false);
            public IntPtr Buffer;
            public Bitmap Frame;
            public int TryCount;
            public int ErrorShown;      // 0/1, show at most one error message per run
            // keep delegates referenced while LibVLC can call them
            public MediaPlayer.LibVLCVideoLockCb Lock;
            public MediaPlayer.LibVLCVideoUnlockCb Unlock;
            public MediaPlayer.LibVLCVideoDisplayCb Display;
        }

        private Run current;

        public override void Start()
        {
            var run = new Run();
            run.Buffer = Marshal.AllocHGlobal(bufferSize);
            run.Frame = new Bitmap((int)width, (int)height, (int)pitch, PixelFormat.Format32bppRgb, run.Buffer);
            run.Lock = (opaque, planes) => { Marshal.WriteIntPtr(planes, run.Buffer); return run.Buffer; };
            run.Unlock = (opaque, picture, planes) => { };
            run.Display = (opaque, picture) => OnDisplay(run);

            current = run;
            stopEvent = run.Stop;

            thread = new Thread(() => WorkerThread(run));
            thread.Name = String.Format("M3U8[{0}]", cameraAdapter.CameraName);
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// Signal the worker to stop and return immediately.
        /// Joining here blocked the UI thread while LibVLC stopped / reconnected the stream.
        /// </summary>
        public override void Stop()
        {
            Run run = current;
            current = null;
            thread = null;
            run?.Stop.Set();
        }

        public override void WorkerThread()
        {
            Run run = current;
            if (run != null)
                WorkerThread(run);
        }

        private void WorkerThread(Run run)
        {
            try
            {
                using (MediaPlayer mediaPlayer = new MediaPlayer(SharedLibVLC))
                {
                    mediaPlayer.SetVideoFormat("RV32", width, height, pitch);
                    mediaPlayer.SetVideoCallbacks(run.Lock, run.Unlock, run.Display);

                    string playlistUrl = cameraAdapter.SourcePath;

                    // LibVLC event thread: must not block here (no modal UI, no LibVLC calls)
                    mediaPlayer.EncounteredError += (sender, e) =>
                    {
                        ShowError(run, String.Format("Failed to play playback.\nCamera[{0}]", cameraAdapter.CameraName));
                        run.Stop.Set();
                    };

                    using (var media = new Media(SharedLibVLC, playlistUrl, FromType.FromLocation))
                    {
                        ////media.AddOption(":rate=0.1");
                        ////media.AddOption(":live-caching=300");
                        media.AddOption(":drop-late-frames");
                        if (!mediaPlayer.Play(media))
                        {
                            ShowError(run, String.Format("Failed to start playback.\nCamera[{0}]", cameraAdapter.CameraName));
                            run.Stop.Set();
                        }
                    }

                    while (!run.Stop.WaitOne(_restart_time))
                    {
                        if (mediaPlayer.State == VLCState.Ended ||
                            mediaPlayer.State == VLCState.Error ||
                            !mediaPlayer.IsPlaying ||
                            run.TryCount == 5)
                        {
                            RestartStream(mediaPlayer, playlistUrl, run);
                            run.TryCount = 0;
                        }
                        run.TryCount++;
                    }

                    // may take a while for network streams - runs on this worker, not on the UI thread
                    mediaPlayer.Stop();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                // LibVLC no longer calls the video callbacks after Stop/Dispose
                run.Frame?.Dispose();
                Marshal.FreeHGlobal(run.Buffer);
                run.Stop.Dispose();
            }
        }

        private void RestartStream(MediaPlayer mediaPlayer, string playlistUrl, Run run)
        {
            if (run.Stop.WaitOne(0))
                return;
            try
            {
                Console.WriteLine(String.Format("Restarting stream.Camera [{0}]", cameraAdapter.CameraName));

                mediaPlayer.Stop();
                if (run.Stop.WaitOne(0))
                    return;
                using (var media = new Media(SharedLibVLC, playlistUrl, FromType.FromLocation))
                {
                    //media.AddOption(":rate=0.1");
                    //media.AddOption(":live-caching=300");
                    media.AddOption(":drop-late-frames");
                    mediaPlayer.Play(media);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Camera [{0}]Restart failed:{1}",cameraAdapter.CameraName, ex.Message));
            }
        }

        /// <summary>
        /// Show error without blocking the caller (worker / LibVLC thread), once per run
        /// </summary>
        private static void ShowError(Run run, string message)
        {
            Console.WriteLine(message);
            if (Interlocked.Exchange(ref run.ErrorShown, 1) == 0)
                ThreadPool.QueueUserWorkItem(_ => MessageBox.Show(message, "M3U8",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning));
        }

        private void SafeCopy(ref Bitmap safeCopy)
        {
            BitmapData bmpData = safeCopy.LockBits(
               new Rectangle(0, 0, safeCopy.Width, safeCopy.Height),
               ImageLockMode.ReadOnly,
               safeCopy.PixelFormat);

            int bytesPerPixel = Image.GetPixelFormatSize(safeCopy.PixelFormat) / 8;
            int stride = bmpData.Stride;

            unsafe
            {
                byte* ptr = (byte*)bmpData.Scan0;

                for (int y = 0; y < safeCopy.Height; y++)
                {
                    for (int x = 0; x < safeCopy.Width; x++)
                    {
                        byte blue = ptr[0];
                        byte green = ptr[1];
                        byte red = ptr[2];
                        byte alpha = ptr[3];

                        ptr += bytesPerPixel;
                    }
                    ptr += stride - (safeCopy.Width * bytesPerPixel);
                }
            }

            safeCopy.UnlockBits(bmpData);
        }

        private Bitmap SafeCopy(Bitmap source)
        {
            Bitmap copy = new Bitmap(source.Width, source.Height, source.PixelFormat);

            BitmapData srcData = source.LockBits(
                new Rectangle(0, 0, source.Width, source.Height),
                ImageLockMode.ReadOnly,
                source.PixelFormat);

            BitmapData dstData = copy.LockBits(
                new Rectangle(0, 0, copy.Width, copy.Height),
                ImageLockMode.WriteOnly,
                copy.PixelFormat);

            int bytes = Math.Abs(srcData.Stride) * source.Height;

            unsafe
            {
                Buffer.MemoryCopy(
                    srcData.Scan0.ToPointer(),
                    dstData.Scan0.ToPointer(),
                    bytes,
                    bytes);
            }

            source.UnlockBits(srcData);
            copy.UnlockBits(dstData);

            return copy;
        }

        private void OnDisplay(Run run)
        {
            try
            {
                run.TryCount = 0;
                Bitmap safeCopy = run.Frame;
                SafeCopy(ref safeCopy);

                if ((cameraAdapter.SaveToFile) && (!run.Stop.WaitOne(0, true)))
                {
                    if (cameraAdapter.UseYOLO)
                    {
                        yoloCopy = SafeCopy(safeCopy);
                        DetectObjects(yoloCopy);
                    }
                    else if (cameraAdapter.MoviDetect)
                    {
                        if (DetectMotion(ref safeCopy))
                        {
                            SaveToFileJpeg(safeCopy);
                        }
                    }
                    else
                    {
                        SaveToFileJpeg(safeCopy);
                    }
                }
                NewFrame?.Invoke(this, new CameraEventArgs(safeCopy));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
