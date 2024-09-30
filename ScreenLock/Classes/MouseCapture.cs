
using System.Runtime.InteropServices;

namespace ScreenLock.Classes
{
    public partial class MouseCapture : IDisposable
    {
        private int padding = 10;
        private CancellationTokenSource cancellationTokenSource;
        private volatile bool isPaused;

        public int Padding
        {
            get => padding;
            set
            {
                if (value >= 0)
                    padding = value;
            }
        }

        public bool IsPaused => isPaused;

        private const int MillisecondsDelay = 100;
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [DllImport("user32.dll")]
        private static extern int ClipCursor(ref RECT lpRect);

        public static int SetCursorClip(ref RECT rect) => ClipCursor(ref rect);

        [DllImport("user32.dll")]
        private static extern  int ClipCursor(IntPtr lpRect);


        public MouseCapture()
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task StartCapturingAsync()
        {
            cancellationTokenSource = new CancellationTokenSource();
            try
            {
                while (!cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (!isPaused)
                    {
                        CaptureMouse();
                    }
                    else
                    {
                        ReleaseMouse();
                    }
                    await Task.Delay(MillisecondsDelay);
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in StartCapturingAsync: {ex.Message}");
            }
        }

        public void StopCapturing()
        {
            cancellationTokenSource.Cancel();
            ReleaseMouse();
        }

        public void PauseCapturing() => isPaused = true;

        public void ResumeCapturing() => isPaused = false;

        private void CaptureMouse()
        {
            try
            {
                var cursorPosition = Cursor.Position;
                var screenBounds = Screen.FromPoint(cursorPosition).Bounds;

                RECT rect = new()
                {
                    left = screenBounds.Left + padding,
                    top = screenBounds.Top + padding,
                    right = screenBounds.Right - padding,
                    bottom = screenBounds.Bottom - padding
                };

                ClipCursor(ref rect);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CaptureMouse: {ex.Message}");
            }
        }

        private static void ReleaseMouse()
        {
            try
            {
                ClipCursor(IntPtr.Zero);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ReleaseMouse: {ex.Message}");
            }
        }

        public void UpdatePadding(int newPadding)
        {
            Padding = newPadding;
        }
        public void Dispose()
        {
            StopCapturing();
            GC.SuppressFinalize(this);
        }

    }
}
