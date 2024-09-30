using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

namespace ScreenLock.Classes
{
    public class SettingsManager
    {
        private ModifierKeys currentModifiers;
        private Keys currentKey;
        private int currentPadding;
        private bool settingsLoaded = false;

        public ModifierKeys CurrentModifiers
        {
            get => currentModifiers;
            set => currentModifiers = value;
        }

        public Keys CurrentKey
        {
            get => currentKey;
            set => currentKey = value;
        }

        public int CurrentPadding
        {
            get => currentPadding;
            set => currentPadding = value;
        }

        public class SettingsData
        {
            public int Modifier { get; set; }
            public int Key { get; set; }
            public int Padding { get; set; }
        }

        string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ScreenLockOverlay");

        public event Action? SettingsLoaded;

        public SettingsManager() => LoadSettings();

        public void LoadSettings()
        {
            try
            {
                string filePath = Path.Combine(directoryPath, "settings.json");

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var settingsData = JsonSerializer.Deserialize<SettingsData>(json);

                    if (settingsData != null)
                    {
                        currentModifiers = (ModifierKeys)settingsData.Modifier;
                        currentKey = (Keys)settingsData.Key;
                        currentPadding = settingsData.Padding;

                        settingsLoaded = true;
                        SettingsLoaded?.Invoke();
                    }
                }
                else
                {
                    Console.WriteLine("Settings file not found. Using default settings.");
                    currentModifiers = ModifierKeys.Shift;
                    currentKey = Keys.F4; 
                    currentPadding = 10; 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings: {ex.Message}");
                settingsLoaded = false;
            }
        }

        public void SaveSettings()
        {
            if (!settingsLoaded)
            {
                return;
            }
            try
            {
                Debug.WriteLine(directoryPath);
                var settingsData = new SettingsData
                {
                    Modifier = (int)currentModifiers,
                    Key = (int)currentKey,
                    Padding = currentPadding
                };

                // Ensure the directory exists
                Directory.CreateDirectory(directoryPath);

                // Specify the file path
                string filePath = Path.Combine(directoryPath, "settings.json");

                // If the file does not exist, create it
                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Dispose(); // Create and close the file
                }

                // Serialize settings to JSON and write to the file
                string json = JsonSerializer.Serialize(settingsData);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public void SetHotkeySettings(string modifierText, string hotkeyText)
        {
            try
            {
                CurrentModifiers = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), modifierText);
                CurrentKey = (Keys)Enum.Parse(typeof(Keys), hotkeyText);

                settingsLoaded = true;
                SaveSettings();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
            }
        }

        public void UpdatePadding(int newPadding)
        {
            if (newPadding >= 0)
            {
                currentPadding = newPadding;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(newPadding), "Padding must be non-negative.");
            }
        }

        public void DeleteTemporaryFile()
        {
            settingsLoaded = false;

            if (Directory.Exists(directoryPath))
            {
                foreach (var file in Directory.GetFiles(directoryPath))
                {
                    File.Delete(file);
                }

                // Clear all subdirectories
                foreach (var dir in Directory.GetDirectories(directoryPath))
                {
                    Directory.Delete(dir, true); 
                }

                Directory.Delete(directoryPath);
            }
        }
    }
}
