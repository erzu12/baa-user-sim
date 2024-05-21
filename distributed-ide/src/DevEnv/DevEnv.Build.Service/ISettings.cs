namespace DevEnv.Build.Service
{
    /// <summary>
    /// Provides the settings for the Build Service.
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Gets the main data directory for the application.
        /// </summary>
        string DataDirectoryPath { get; }
    }
}
