
namespace DevEnv.Base.Utilities
{
    /// <summary>
    /// Provides utility methods related to data directories.
    /// </summary>
    public static class DataDirectoryUtils
    {
        /// <summary>
        /// Gets a default data directory for the application.
        /// </summary>
        /// <returns></returns>
        public static string GetDataDirectory()
        {
            var appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appName = AppDomain.CurrentDomain.FriendlyName;

            var dataDirectoryPath = Path.Combine(appDataFolderPath, appName);

            return dataDirectoryPath;
        }
    }
}
