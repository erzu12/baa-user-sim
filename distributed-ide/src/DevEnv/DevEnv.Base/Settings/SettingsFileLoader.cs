
using System.Text.Json;
using DevEnv.Base.RuntimeChecks;

namespace DevEnv.Base.Settings
{
    /// <summary>
    /// Implements <see cref="ISettingsProvider{TSettings}"/>. Loads the settings from a JSON file. If the settings file
    /// does not exist, it creates a settings file with default settings.
    /// </summary>
    /// <typeparam name="TSettings">
    /// The settings type
    /// </typeparam>
    public class SettingsFileLoader<TSettings> : ISettingsProvider<TSettings>
        where TSettings : class, new()
    {
        private readonly Lazy<TSettings> lazySettings;

        public SettingsFileLoader(string fileName)
        {
            Argument.AssertNotEmpty(fileName, nameof(fileName));

            this.lazySettings = new Lazy<TSettings>(() => LoadOrCreateDefault(fileName), isThreadSafe: true);
        }

        public TSettings GetSettings()
        {
            return this.lazySettings.Value;
        }

        private static TSettings LoadOrCreateDefault(string fileName)
        {
            if (!File.Exists(fileName))
            {
                // Create a settings file with default settings.
                var defaultSettings = new TSettings();
                var json = JsonSerializer.Serialize(defaultSettings);

                File.WriteAllText(fileName, json);
            }

            var settings = LoadFromFile(fileName);

            return settings;
        }

        private static TSettings LoadFromFile(string fileName)
        {
            Argument.AssertNotEmpty(fileName, nameof(fileName));

            var json = File.ReadAllText(fileName);
            var settings = JsonSerializer.Deserialize<TSettings>(json);

            if (settings == null)
            {
                throw new InvalidDataException("The settings file could not be deserialized.");
            }

            return settings;
        }
    }
}
