
namespace DevEnv.Build.Client
{
    /// <summary>
    /// Represents the result of a build process.
    /// </summary>
    public class BuildResult
    {
        public BuildResult(string buildId, bool isSuccessful, string output)
        {
            this.BuildId = buildId;
            this.IsSuccessful = isSuccessful;
            this.Output = output;
        }

        /// <summary>
        /// Gets a value indicating whether the build was successful.
        /// </summary>
        public bool IsSuccessful { get; }

        /// <summary>
        /// Gets a unique for the build.
        /// </summary>
        public string BuildId { get; }

        /// <summary>
        /// Gets the build output.
        /// </summary>
        public string Output { get; }
    }
}
