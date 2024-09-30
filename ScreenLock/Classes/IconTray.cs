using System.IO;

namespace ScreenLock.Classes
{ 
    public class IconTray : IDisposable
    {
        private NotifyIcon? notifyIcon;
        private ContextMenuStrip? contextMenu;
        private readonly Action onOpen;
        private readonly Action onToggle;
        private readonly Action onExit;

        private bool islocked;
        public bool Islocked
        {
            get => islocked;
            set
            {
                islocked = value;
                UpdateLockUnlockMenuItem();
                UpdateTrayIcon();
            }
        }

        private readonly Icon? lockedIcon;
        private readonly Icon? unlockedIcon;
        public IconTray(Action onOpen, Action onExit, Action onToggle)
        {
            string resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

            lockedIcon = new Icon(Path.Combine(resourcePath, "ScreenLocked.ico"), 16, 16);
            unlockedIcon = new Icon(Path.Combine(resourcePath, "ScreenUnlocked.ico"), 16, 16);

            InitializeTrayIcon();
            this.onOpen = onOpen ?? throw new ArgumentNullException(nameof(onOpen));
            this.onToggle = onToggle ?? throw new ArgumentNullException(nameof(onToggle));
            this.onExit = onExit ?? throw new ArgumentNullException(nameof(onExit));
        }

        private void InitializeTrayIcon()
        {
            contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Open", null, (s, e) => onOpen());
            contextMenu.Items.Add("Lock/Unlock", null, (s, e) => onToggle());
            contextMenu.Items.Add("Exit", null, (s, e) => onExit());
            UpdateLockUnlockMenuItem();

            notifyIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                ContextMenuStrip = contextMenu
            };

            notifyIcon.DoubleClick += (s, e) => onOpen();
        }
        private void UpdateLockUnlockMenuItem()
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var lockUnlockItem = contextMenu.Items[1];
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            lockUnlockItem.Text = islocked ? "Unlock" : "Lock";
        }
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        private void UpdateTrayIcon() => notifyIcon.Icon = islocked ? lockedIcon : unlockedIcon;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        public void Dispose()
        {
            notifyIcon?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

}