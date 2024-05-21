namespace DevEnv.WorkDir.Service
{
    /// <summary>
    /// Provides the settings for the Execute Service.
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Gets the main data directory for the application.
        /// </summary>
        string DataDirectoryPath { get; }
    }
}
