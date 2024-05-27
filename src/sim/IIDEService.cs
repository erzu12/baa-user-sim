namespace sim;

using DevEnv.Build.Client;
using GrpcBuild;

interface IIDEService
{
    void setWorkingDirectory(string workDirId);

    string GetWorkingDirectory();

    /// <summary>
    /// Gets a list of files (or file revisions) in the specified working directory.
    /// </summary>
    /// </param>
    /// <returns>
    /// A list of files
    /// </returns>
    IEnumerable<string> GetFiles();

    /// <summary>
    /// Loads the file content of the specified file from the specified working directory.
    /// </summary>
    /// <param name="filePath">
    /// The file, as relative path within the working directory
    /// </param>
    /// <returns>
    /// The file content, as byte array
    /// </returns>
    byte[] LoadFile(string filePath);

    /// <summary>
    /// Updates the content of the specified file in the specified working directory.
    /// </summary>
    /// <param name="filePath">
    /// The file, as relative path within the working directory
    /// </param>
    /// <param name="updatedFileContent">
    /// The new updated file content, as byte array
    /// </param>
    /// <returns>
    /// An awaitable task
    /// </returns>
    void UpdateFile(string filePath, byte[] updatedFileContent);

    /// <summary>
    /// Commits the changes in the specified working directory.
    /// </summary>
    /// <param name="commitMessage">
    /// The commit message
    /// </param>
    /// <returns>
    /// An awaitable task
    /// </returns>
    void CommitChanges(string commitMessage);


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
    BuildResult Build(BuildSystem buildSystem, string rootFile);

}


