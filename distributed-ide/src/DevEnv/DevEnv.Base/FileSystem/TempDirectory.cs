using System.Diagnostics;
using DevEnv.Base.RuntimeChecks;

namespace DevEnv.Base.FileSystem
{
    /// <summary>
    /// Creates a random temporary directory which can be used as disposable working directory.
    /// </summary>
    public class TempDirectory : IDisposable
    {
        /// <summary>
        /// Initializes and created the random temporary directory.
        /// </summary>
        public TempDirectory()
            : this(pathPrefix: Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Process.GetCurrentProcess().ProcessName))
        {
        }

        /// <summary>
        /// Initializes and created the random temporary directory, using the specified path prefix.
        /// </summary>
        /// <param name="pathPrefix">
        /// The path prefix where the random temporary directory is created
        /// </param>
        public TempDirectory(string pathPrefix)
        {
            Argument.AssertNotNull(pathPrefix, nameof(pathPrefix));

            var folderName = Guid.NewGuid().ToString("B");
            var path = Path.Combine(pathPrefix, folderName);

            // Create the directory.
            var dir = Directory.CreateDirectory(path);
            this.FullName = dir.FullName;
        }

        /// <summary>
        /// Gets the full name of the temporary directory.
        /// </summary>
        public string FullName { get; }

        /// <summary>
        /// Disposes of the temporary directory.
        /// </summary>
        public void Dispose()
        {
            try
            {
                Directory.Delete(this.FullName, recursive: true);
            }
            catch (Exception) { };
        }
    }
}