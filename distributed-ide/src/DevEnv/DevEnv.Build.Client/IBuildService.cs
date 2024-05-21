
using GrpcBuild;

namespace DevEnv.Build.Client
{
    /// <summary>
    /// The client API for the Build Service.
    /// </summary>
    public interface IBuildService
    {
        /// <summary>
        /// Builds the specified .NET solution from the specified working directory.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory ID
        /// </param>
        /// <param name="solutionFile">
        /// The solution file, as relative path within the working directory
        /// </param>
        /// <returns>
        /// A build result
        /// </returns>
        Task<BuildResult> BuildDotnet(string workDirId, string solutionFile);

        /// <summary>
        /// Builds the specified .NET solution from the specified working directory.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory ID
        /// </param>
        /// <param name="buildSystem">
        /// The build system
        /// </param>
        /// <param name="rootFile">
        /// The build definition root file, as relative path within the working directory
        /// </param>
        /// <returns>
        /// A build result
        /// </returns>
        Task<BuildResult> Build(string workDirId, BuildSystem buildSystem, string rootFile);

        /// <summary>
        /// Gets a list of build artifact files (or file revisions) from a specified working directory and build.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory ID
        /// </param>
        /// <param name="buildId">
        /// The build ID
        /// </param>
        /// <returns>
        /// A list of build artifact files
        /// </returns>
        Task<IEnumerable<FileRevision>> GetArtifactFiles(string workDirId, string buildId);

        /// <summary>
        /// Loads the file content of the specified file from the specified working directory and build.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory ID
        /// </param>
        /// <param name="buildId">
        /// The build ID
        /// </param>
        /// <param name="filePath">
        /// The file, as relative path within the artifacts directory
        /// </param>
        /// <returns>
        /// The file content, as byte array
        /// </returns>
        Task<byte[]> LoadArtifactFile(string workDirId, string buildId, string filePath);
    }
}
