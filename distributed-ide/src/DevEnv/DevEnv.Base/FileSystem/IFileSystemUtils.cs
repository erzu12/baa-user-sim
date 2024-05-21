
namespace DevEnv.Base.FileSystem
{
    /// <summary>
    /// Provides utility methods for interacting with the file system.
    /// </summary>
    public interface IFileSystemUtils
    {
        /// <summary>
        /// Creates a directory from a path, if it does not already exist.
        /// </summary>
        /// <param name="path">
        /// The path
        /// </param>
        void CreateDirectoryIfNecessary(string path);

        /// <summary>
        /// Determines whether two specified files stored on the file system are identical in content.
        /// </summary>
        /// <param name="filePathA">
        /// The path of file A
        /// </param>
        /// <param name="filePathB">
        /// The path of file B
        /// </param>
        /// <returns>
        /// A value indicating whether the specified files are identical in content
        /// </returns>
        bool AreFilesEqual(string filePathA, string filePathB);
    }
}
