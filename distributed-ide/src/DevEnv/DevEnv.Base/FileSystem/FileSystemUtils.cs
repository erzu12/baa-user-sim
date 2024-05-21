
using DevEnv.Base.RuntimeChecks;
using System.IO;

namespace DevEnv.Base.FileSystem
{
    public class FileSystemUtils : IFileSystemUtils
    {
        public void CreateDirectoryIfNecessary(string path)
        {
            Argument.AssertNotEmpty(path, nameof(path));

            var directoryPath = Path.GetDirectoryName(path);

            if (string.IsNullOrEmpty(directoryPath))
            {
                throw new ArgumentException($"The specified path (\"{path}\") does not contain a directory");
            }

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public bool AreFilesEqual(string filePathA, string filePathB)
        {
            Argument.AssertNotEmpty(filePathA, nameof(filePathA));
            Argument.AssertNotEmpty(filePathB, nameof(filePathB));

            if (!File.Exists(filePathA))
            {
                throw new ArgumentException($"File \"{filePathA}\" does not exist.");
            }

            if (!File.Exists(filePathB))
            {
                throw new ArgumentException($"File \"{filePathB}\" does not exist.");
            }

            using var streamA = File.OpenRead(filePathA);
            using var streamB = File.OpenRead(filePathB);

            // Check length.

            if (streamA.Length != streamB.Length)
            {
                return false;
            }

            // Read through files and compare bytewise.
            while (streamA.Position < streamA.Length &&
                   streamB.Position < streamB.Length)
            {
                var byteA = streamA.ReadByte();
                var byteB = streamB.ReadByte();

                if (byteA != byteB)
                {
                    return false;
                }
            }

            // We haven't found a difference.
            return true;
        }
    }
}
