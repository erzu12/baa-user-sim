using DevEnv.Build.Service.BuildArtifacts.MetaData;

namespace DevEnv.Build.Service.BuildArtifacts
{
    /// <summary>
    /// Provides helper methods for managing build artifacts.
    /// </summary>
    public interface IBuildArtifactsHelper
    {
        /// <summary>
        /// Updates and persists the total artifacts metadata over an entire work dir.
        /// </summary>
        void UpdatePersistTotalArtifactsMetadata(string workDirId, Action<BuildArtifactsMetadata> updateMetadataAction);

        /// <summary>
        /// Gets the total artifacts metadata over an entire work dir.
        /// </summary>
        BuildArtifactsMetadata GetTotalArtifactsMetadata(string workDirId);

        /// <summary>
        /// Updates and persists the metadata for the artifacts of a specific build.
        /// </summary>
        void UpdatePersistBuildSpecificArtifactsMetadata(string workDirId, string buildId, Action<BuildArtifactsMetadata> updateMetadataAction);

        /// <summary>
        /// Gets the metadata for the artifacts of a specific build.
        /// </summary>
        BuildArtifactsMetadata GetBuildSpecificArtifactsMetadata(string workDirId, string buildId);

        /// <summary>
        /// Gets the directory where all the source files are cached.
        /// </summary>
        string GetCacheDirectoryPath(string workDirId);

        /// <summary>
        /// Gets the base directory where temporary build directories are created for each build.
        /// </summary>
        string GetBuildBaseDirectoryPath(string workDirId);

        /// <summary>
        /// Gets the base directory where build artifact directories are created for each build.
        /// </summary>
        string GetArtifactsBaseDirectoryPath(string workDirId);

        /// <summary>
        /// Gets the build artigacts directory for build artifacts of a specific build.
        /// </summary>
        string GetBuildArtifactsDirectoryPath(string workDirId, string buildId);
    }
}
