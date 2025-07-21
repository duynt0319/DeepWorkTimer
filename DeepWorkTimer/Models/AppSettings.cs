using System.IO;
using System.Text.Json;

namespace DeepWorkTimer.Models
{
    /// <summary>
    /// Model containing all application settings
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Preferred monitor index (0-based)
        /// </summary>
        public int PreferredScreenIndex { get; set; } = 0;

        /// <summary>
        /// Window X position
        /// </summary>
        public double WindowLeft { get; set; } = 100;

        /// <summary>
        /// Window Y position
        /// </summary>
        public double WindowTop { get; set; } = 100;

        /// <summary>
        /// Transparency level (0.0 - 1.0)
        /// </summary>
        public double Opacity { get; set; } = 0.85;

        /// <summary>
        /// Whether Global Hotkeys are enabled
        /// </summary>
        public bool GlobalHotkeysEnabled { get; set; } = true;

        /// <summary>
        /// Notification display duration (milliseconds)
        /// </summary>
        public int NotificationDuration { get; set; } = 2000;

        /// <summary>
        /// Settings file path
        /// </summary>
        public static string SettingsFilePath
        {
            get
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appFolder = Path.Combine(appDataPath, "DeepWorkTimer");
                Directory.CreateDirectory(appFolder); // Create directory if it doesn't exist
                return Path.Combine(appFolder, "settings.json");
            }
        }

        /// <summary>
        /// Save settings to file
        /// </summary>
        public void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(SettingsFilePath, json);
                System.Diagnostics.Debug.WriteLine($"? Settings saved to: {SettingsFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Failed to save settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Load settings from file
        /// </summary>
        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    var json = File.ReadAllText(SettingsFilePath);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"? Settings loaded from: {SettingsFilePath}");
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? Failed to load settings: {ex.Message}");
            }

            // Return default settings if unable to load
            System.Diagnostics.Debug.WriteLine("?? Using default settings");
            return new AppSettings();
        }
    }
}