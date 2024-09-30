using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace ScreenLock.Classes
{
    public partial class HotkeyManager(IntPtr windowHandle) : IDisposable
    {
        // Windows API functions
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HotkeyId = 1;
        private const uint ModControl = 0x0002; // Control key
        private const uint ModShift = 0x0004;   // Shift key
        private const uint ModAlt = 0x0001;     // Alt key
        private bool _isDisposed = false;

        // Declare the event
        public event Action? ToggleOverlay;

        public void RegisterHotkey(ModifierKeys modifier, Keys key)
        {
            uint fsModifiers = 0;
            UnRegisterHotkey();

            // Set the modifier flags based on the provided modifier keys
            if (modifier.HasFlag(ModifierKeys.Control))
                fsModifiers |= ModControl;
            if (modifier.HasFlag(ModifierKeys.Shift))
                fsModifiers |= ModShift;

            // Register the hotkey
            if (!RegisterHotKey(windowHandle, HotkeyId, fsModifiers, (uint)key))
            {
                throw new InvalidOperationException("Hotkey registration failed.");
            }
        }

        public void ProcessHotKeyMessage(Message message)
        {
            if (message.Msg == 0x0312 && message.WParam.ToInt32() == HotkeyId)
            {
                ToggleOverlay?.Invoke(); // Trigger the event
            }
        }
        public void UnRegisterHotkey()
        {
            UnregisterHotKey(windowHandle, HotkeyId);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                UnregisterHotKey(windowHandle, HotkeyId);
                _isDisposed = true;
            }
        }
    }
}
