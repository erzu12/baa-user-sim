
namespace DevEnv.WorkDir.Client
{
    /// <summary>
    /// Represents a specific file revision.
    /// </summary>
    public class FileRevision
    {
        public FileRevision(string fileName, int revision)
        {
            this.FileName = fileName;
            this.Revision = revision;
        }

        /// <summary>
        /// Gets the file name, which identifies a specific file.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets the revision number of the file.
        /// </summary>
        public int Revision { get; }
    }
}
