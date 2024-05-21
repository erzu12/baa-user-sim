namespace DevEnv.Execute.Client
{
    /// <summary>
    /// Provides settings for the Execute Service client.
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Gets the Execute Service address.
        /// </summary>
        string ExecuteServiceAddress { get; }
    }
}
