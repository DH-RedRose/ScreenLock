using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using ScreenLock.Classes;
using ScreenLock.Properties;

namespace ScreenLock
{
    public partial class MainWindow : Window
    {
        private readonly IconTray iconTray;
        private readonly MouseCapture mouseCapture;
        private readonly SettingsManager settingsManager;
        private HotkeyManager hotkeyManager;

        private const int MinValue = 0;
        private const int MaxValue = 20;

        private bool isSettingHotkey = false;
        private Key? newHotkey = null;
        private ModifierKeys modifierKey = ModifierKeys.None;

        public MainWindow()
        {
            InitializeComponent();
            mouseCapture = new MouseCapture();
            iconTray = new IconTray(OnOpen, OnExit, OnToggle);
            settingsManager = new SettingsManager();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
            this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;

            settingsManager.LoadSettings();

            hotkeyManager.RegisterHotkey(settingsManager.CurrentModifiers, settingsManager.CurrentKey);

            StartCapture();
            LockToggleButton.IsChecked = true;
            SetPaddingValue((int)settingsManager.CurrentPadding);
            HotkeyLabel.Content = $"{settingsManager.CurrentModifiers} + {settingsManager.CurrentKey}";
        }
        private void Window_Closed(object sender, System.EventArgs e)
        {
            Cleanup();
        }

        private void Cleanup()
        {
            settingsManager.CurrentPadding = GetPaddingValue();
            settingsManager.SaveSettings();
            mouseCapture.Dispose(); 
            hotkeyManager?.Dispose();
            iconTray?.Dispose();
        }
        private async Task StartMouseCapture()
        {
            await mouseCapture.StartCapturingAsync();
        }

        private async void StartCapture()
        {
            await StartMouseCapture();
        }
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource? source = PresentationSource.FromVisual(this) as HwndSource;
            source?.AddHook(WndProc);

            hotkeyManager = new HotkeyManager(new WindowInteropHelper(this).Handle);
            hotkeyManager.ToggleOverlay += OverlayToggle;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            hotkeyManager.ProcessHotKeyMessage(new Message { Msg = msg, WParam = wParam, LParam = lParam });
            return IntPtr.Zero;
        }

        private void OverlayToggle()
        {
            if (HotkeyActionToggleButton.IsChecked == true)
            {
                OverlayGrid.Visibility = OverlayGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                LockToggleButton.IsChecked = !LockToggleButton.IsChecked;
                iconTray.Islocked = (bool)LockToggleButton.IsChecked;
            }
        }

        #region Hotkey SetUp
        private void SetHotkeyButton_Click(object sender, RoutedEventArgs e)
        {
            hotkeyManager.UnRegisterHotkey();
            if (isSettingHotkey)
            {
                string modifierText = modifierKey.ToString();
                string hotkeyText = newHotkey.HasValue ? newHotkey.Value.ToString() : string.Empty;
                try
                {
                    settingsManager.SetHotkeySettings(modifierText, hotkeyText);
                    hotkeyManager.RegisterHotkey(settingsManager.CurrentModifiers, settingsManager.CurrentKey);
                    HotkeyLabel.Content = $"{settingsManager.CurrentModifiers} + {settingsManager.CurrentKey}";
                    settingsManager.SaveSettings();
                    SetHotkeyButton.Content = "Set Hotkey";
                    DeleteButton.Content = "Delete Settings";
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error: {ex.Message}");
                }
            }
            else
            {
                HotkeyLabel.Content = "...";
                SetHotkeyButton.Content = "Confirm Keys";
            }
            isSettingHotkey = !isSettingHotkey;
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (isSettingHotkey)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    modifierKey = ModifierKeys.Control;
                }
                else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    modifierKey = ModifierKeys.Shift;
                }
                else if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
                {
                    e.Handled = true;
                    return;
                }
                else
                {
                    modifierKey = ModifierKeys.None;
                }
                if (e.Key != Key.System)
                {
                    newHotkey = e.Key;
                    HotkeyLabel.Content = $"{modifierKey} + {newHotkey.Value}";
                }
                e.Handled = true;
            }

            base.OnKeyDown(e);
        }
        #endregion

        #region Tray
        private void OnOpen()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
            OverlayGrid.Visibility = Visibility.Visible;
        }

        private void OnToggle()
        {
            LockToggleButton.IsChecked = !LockToggleButton.IsChecked;
            iconTray.Islocked = (bool)LockToggleButton.IsChecked;
        }

        private void OnExit()
        {
            Cleanup();
            System.Windows.Application.Current.Shutdown();
        }

        #endregion

        #region Menu Buttons
        private void LockToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            mouseCapture.ResumeCapturing();
            LockToggleButton.Content = "Unlock";
            iconTray.Islocked = true;
        }

        private void LockToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            mouseCapture.PauseCapturing();
            LockToggleButton.Content = "Lock";
            iconTray.Islocked = false;
        }
        private void HotkeyActionToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            HotkeyActionToggleButton.Content = "Hotkey Action: UI";
        }
        private void HotkeyActionToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {

            HotkeyActionToggleButton.Content = "Hotkey Action: Lock";
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            OverlayGrid.Visibility =  Visibility.Collapsed;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Cleanup();
            System.Windows.Application.Current.Shutdown();
        }
        private void DeleteSettings_Click(object sender, RoutedEventArgs e)
        {
            settingsManager.DeleteTemporaryFile();
            DeleteButton.Content = "Settings Deleted";
            HotkeyLabel.Content = "Hotkey Cleared!";

            isSettingHotkey = false;
            SetHotkeyButton.Content = "Set Hotkey";
        }
        #endregion

        #region Padding Scale
        private int GetPaddingValue()
        {
            if (int.TryParse(PaddingValueTextBlock.Text, out int value))
            {
                return value;
            }
            return 10;
        }

        private void SetPaddingValue(int value)
        {
            if (value < 0) value = 0;
            if (value > 20) value = 20;

            UpdateButtonStates(value);
            PaddingValueTextBlock.Text = value.ToString();
            mouseCapture.UpdatePadding(value);
        }
        private void IncreaseButton_Click(object sender, RoutedEventArgs e)
        {
            int currentPadding = GetPaddingValue();
            if (currentPadding < MaxValue)
            {
                SetPaddingValue(currentPadding + 1);
                settingsManager.CurrentPadding = currentPadding + 1;
            }
        }
        private void DecreaseButton_Click(object sender, RoutedEventArgs e)
        {
            int currentPadding = GetPaddingValue();
            if (currentPadding > MinValue)
            {
                SetPaddingValue(currentPadding - 1);
                settingsManager.CurrentPadding = currentPadding - 1;
            }
        }

        private void UpdateButtonStates(int value)
        {
            DecreaseButton.IsEnabled = value != MinValue;
            IncreaseButton.IsEnabled = value != MaxValue;
        }


        #endregion


    }
}
