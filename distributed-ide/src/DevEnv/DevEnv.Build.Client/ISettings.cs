
namespace DevEnv.Build.Client
{
    /// <summary>
    /// Provides settings for the Build Service client.
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Gets the Build Service address.
        /// </summary>
        string BuildServiceAddress { get; }
    }
}
