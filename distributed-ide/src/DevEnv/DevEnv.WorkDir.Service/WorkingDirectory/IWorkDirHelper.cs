using DevEnv.WorkDir.Service.WorkingDirectory.MetaData;

namespace DevEnv.WorkDir.Service.WorkingDirectory
{
    /// <summary>
    /// Provides helper methods for managing working directories.
    /// </summary>
    public interface IWorkDirHelper
    {
        /// <summary>
        /// Updates and persists the metadata of the specified working directory.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory ID
        /// </param>
        /// <param name="updateMetadataAction">
        /// An action which updates the metadata instance
        /// </param>
        void UpdatePersistMetadata(string workDirId, Action<WorkDirMetadata> updateMetadataAction);

        /// <summary>
        /// Gets the metadata of the specified working directory.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory ID
        /// </param>
        /// <returns>
        /// The working directory metadata 
        /// </returns>
        WorkDirMetadata GetMetadata(string workDirId);

        /// <summary>
        /// Prepares the working directory path, based on the specified ID, and verifies (asserts) that the directory exists.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory ID
        /// </param>
        /// <returns>
        /// The working directory path
        /// </returns>
        string PrepareAndVerifyWorkDirPath(string workDirId);

        /// <summary>
        /// Prepares the repository directory path, based on the specified working directory ID, and verifies (asserts) that the directory exists.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory ID
        /// </param>
        /// <returns>
        /// The repository directory path
        /// </returns>
        string PrepareAndVerifyRepoDirPath(string workDirId);

        /// <summary>
        /// Prepares the full file path, based on the specified working directory ID and relative file path, and verifies (asserts) that the file exists.
        /// </summary>
        /// <param name="workDirId">
        /// The working directory ID
        /// </param>
        /// <param name="fileName">
        /// The relative file path
        /// </param>
        /// <returns>
        /// The full file path
        /// </returns>
        string PrepareAndVerifyFilePath(string workDirPath, string fileName);

        /// <summary>
        /// Gets the path of the repository directory within the working directory.
        /// </summary>
        /// <param name="workDirPath">
        /// The working directory path
        /// </param>
        /// <returns>
        /// The repository directory path
        /// </returns>
        string GetRepoDirPath(string workDirPath);

        /// <summary>
        /// Gets the simple name of the repository directory within the working directory.
        /// </summary>
        /// <param name="workDirPath">
        /// The working directory path
        /// </param>
        /// <returns>
        /// The simple repository directory name
        /// </returns>
        string GetRepoDirName(string workDirPath);

        /// <summary>
        /// Determines the current revision number of a file, based on the existing file revision infos.
        /// </summary>
        /// <param name="filePath">
        /// The file path
        /// </param>
        /// <param name="revisionInfos">
        /// The existing file revision infos
        /// </param>
        /// <returns>
        /// The current revision number
        /// </returns>
        int DetermineCurrentRevisionNumber(string filePath, IReadOnlyList<FileRevisionInfo> revisionInfos);

        /// <summary>
        /// Increments the revision of the specified file, and updates the revision infos accordingly.
        /// </summary>
        /// <param name="filePath">
        /// The file path
        /// </param>
        /// <param name="revisionInfosToUpdate">
        /// The list of revision infos, which are to be updated
        /// </param>
        void IncrementRevisionNumber(string filePath, IList<FileRevisionInfo> revisionInfosToUpdate);
    }
}
