namespace DevEnv.Base.Caching
{
    /// <summary>
    /// Keeps track of cached files. Files are identified by file name, unique within
    /// a context.
    /// </summary>
    public interface IFileCacheManager
    {
        /// <summary>
        /// Registers the specified file within the specified context as cached in the specified version.
        /// Only one version per file is cached at a time.
        /// </summary>
        /// <param name="contextId">
        /// The context the file belongs to
        /// </param>
        /// <param name="fileName">
        /// The file name, unique within the context
        /// </param>
        /// <param name="versionNumber">
        /// The current version number
        /// </param>
        void RegisterCachedFile(string contextId, string fileName, int versionNumber);

        /// <summary>
        /// Gets a value indicating wether the specified file within the specified context is cached in the
        /// specified version.
        /// </summary>
        /// <param name="contextId">
        /// The context the file belongs to
        /// </param>
        /// <param name="fileName">
        /// The file name, unique within the context
        /// </param>
        /// <param name="versionNumber">
        /// The current version number
        /// </param>
        /// <returns>
        /// A value indicating whether the file is currently cached
        /// </returns>
        bool IsChached(string contextId, string fileName, int versionNumber);
    }
}
