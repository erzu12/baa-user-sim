
namespace DevEnv.Base.Settings
{
    /// <summary>
    /// Provides access to a settings object of type <see cref="TSettings"/>.
    /// </summary>
    /// <typeparam name="TSettings">
    /// The settings type
    /// </typeparam>
    public interface ISettingsProvider<out TSettings>
        where TSettings : class
    {
        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <returns>
        /// The settings
        /// </returns>
        TSettings GetSettings();
    }
}
