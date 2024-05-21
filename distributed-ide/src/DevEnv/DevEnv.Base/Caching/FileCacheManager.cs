using DevEnv.Base.RuntimeChecks;

namespace DevEnv.Base.Caching
{
    public class FileCacheManager : IFileCacheManager
    {
        private readonly IDictionary<Tuple<string, string>, int> filePathWithRevisionNumbers = new Dictionary<Tuple<string, string>, int>();

        public void RegisterCachedFile(string contextId, string fileName, int versionNumber)
        {
            Argument.AssertNotEmpty(contextId, nameof(contextId));
            Argument.AssertNotEmpty(fileName, nameof(fileName));

            var key = new Tuple<string, string>(contextId, fileName);

            if (this.filePathWithRevisionNumbers.ContainsKey(key))
            {
                this.filePathWithRevisionNumbers[key] = versionNumber;

                return;
            }

            this.filePathWithRevisionNumbers.Add(key, versionNumber);
        }

        public bool IsChached(string contextId, string fileName, int versionNumber)
        {
            Argument.AssertNotEmpty(contextId, nameof(contextId));
            Argument.AssertNotEmpty(fileName, nameof(fileName));

            var key = new Tuple<string, string>(contextId, fileName);

            if (this.filePathWithRevisionNumbers.TryGetValue(key, out var currentlyCachedVersion))
            {
                return currentlyCachedVersion == versionNumber;
            }

            return false;
        }
    }
}
