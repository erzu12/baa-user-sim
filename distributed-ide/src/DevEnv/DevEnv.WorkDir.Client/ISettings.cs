
namespace DevEnv.WorkDir.Client
{
    /// <summary>
    /// Provides settings for the WorkDir Service client.
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Gets the Execute Service address.
        /// </summary>
        string WorkDirServiceAddress { get; }
    }
}
