using DevEnv.Base.RuntimeChecks;

namespace DevEnv.Base.Settings
{
    /// <summary>
    /// Implements <see cref="ISettingsProvider{TSettings}"/>. Simply retrieves the settings using a predefined delegate.
    /// </summary>
    /// <typeparam name="TSettings">
    /// The settings type
    /// </typeparam>
    public class InMemorySettingsProvider<TSettings> : ISettingsProvider<TSettings>
        where TSettings : class
    {
        private Func<TSettings> getSettings;

        public InMemorySettingsProvider(Func<TSettings> getSettings)
        {
            Argument.AssertNotNull(getSettings, nameof(getSettings));

            this.getSettings = getSettings;
        }

        public TSettings GetSettings()
        {
            return this.getSettings();
        }
    }
}
