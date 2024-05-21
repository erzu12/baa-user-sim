namespace DevEnv.WorkDir.Client
{
    /// <summary>
    /// The client API for the WorkDir Service.
    /// </summary>
    public interface IWorkDirService
    {
        /// <summary>
        /// Initializes a new working directory, retrieves a working copy of the specified code repository, and
        /// creating / checks out a specific branch.
        /// </summary>
        /// <param name="description">
        /// A description for the new working directory
        /// </param>
        /// <param name="repoUrl">
        /// The code repository URL
        /// </param>
        /// <param name="branch">
        /// The branch to create / check out
        /// </param>
        /// <returns>
        /// A unique working directory ID
        /// </returns>
        Task<string> StartNewWorkDir(string description, string repoUrl, string branch);

        /// <summary>
        /// Gets a list of files (or file revisions) in the specified working directory.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory
        /// </param>
        /// <returns>
        /// A list of files
        /// </returns>
        Task<IEnumerable<FileRevision>> GetFiles(string workDirId);

        /// <summary>
        /// Loads the file content of the specified file from the specified working directory.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory ID
        /// </param>
        /// <param name="filePath">
        /// The file, as relative path within the working directory
        /// </param>
        /// <returns>
        /// The file content, as byte array
        /// </returns>
        Task<byte[]> LoadFile(string workDirId, string filePath);

        /// <summary>
        /// Updates the content of the specified file in the specified working directory.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory ID
        /// </param>
        /// <param name="filePath">
        /// The file, as relative path within the working directory
        /// </param>
        /// <param name="updatedFileContent">
        /// The new updated file content, as byte array
        /// </param>
        /// <returns>
        /// An awaitable task
        /// </returns>
        Task UpdateFile(string workDirId, string filePath, byte[] updatedFileContent);

        /// <summary>
        /// Commits the changes in the specified working directory.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory ID
        /// </param>
        /// <param name="commitMessage">
        /// The commit message
        /// </param>
        /// <returns>
        /// An awaitable task
        /// </returns>
        Task CommitChanges(string workDirId, string commitMessage);
    }
}