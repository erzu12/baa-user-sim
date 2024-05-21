using DevEnv.Base.FileSystem;
using DevEnv.Base.RuntimeChecks;
using DevEnv.Base.Settings;
using DevEnv.WorkDir.Service.WorkingDirectory.MetaData;

namespace DevEnv.WorkDir.Service.WorkingDirectory
{
    public class WorkDirHelper : IWorkDirHelper
    {
        private readonly ISettings settings;
        private readonly IMetadataFileUtils metadataFileUtils;

        public WorkDirHelper(ISettingsProvider<ISettings> settingsProvider, IMetadataFileUtils metadataFileUtils)
        {
            Argument.AssertNotNull(settingsProvider, nameof(settingsProvider));
            Argument.AssertNotNull(metadataFileUtils, nameof(metadataFileUtils));

            this.settings = settingsProvider.GetSettings();
            this.metadataFileUtils = metadataFileUtils;
        }

        public void UpdatePersistMetadata(string workDirId, Action<WorkDirMetadata> updateMetadataAction)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotNull(updateMetadataAction, nameof(updateMetadataAction));

            var workDirPath = this.PrepareAndVerifyWorkDirPath(workDirId);
            var workDirMetadataFilePath = Path.Combine(workDirPath, ".json");

            this.metadataFileUtils.UpdatePersistMetadata(workDirMetadataFilePath, updateMetadataAction);
        }

        public WorkDirMetadata GetMetadata(string workDirId)
        {
            var workDirPath = this.PrepareAndVerifyWorkDirPath(workDirId);
            var workDirMetadataFilePath = Path.Combine(workDirPath, ".json");

            return this.metadataFileUtils.GetMetadata<WorkDirMetadata>(workDirMetadataFilePath);
        }

        public string PrepareAndVerifyWorkDirPath(string workDirId)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));

            var workDirPath = Path.Combine(this.settings.DataDirectoryPath, workDirId);
            return !Directory.Exists(workDirPath)
                ? throw new ArgumentException($"The specified working directory (ID: {workDirId}) does not exist.")
                : workDirPath;
        }

        public string PrepareAndVerifyRepoDirPath(string workDirId)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));

            var workDirPath = this.PrepareAndVerifyWorkDirPath(workDirId);
            var metadata = this.GetMetadata(workDirId);
            var repoDirPath = Path.Combine(workDirPath, metadata.RepoDirName!);

            return repoDirPath;
        }

        public string PrepareAndVerifyFilePath(string workDirId, string fileName)
        {
            Argument.AssertNotEmpty(workDirId, nameof(workDirId));
            Argument.AssertNotEmpty(fileName, nameof(fileName));

            var repoDirPath = this.PrepareAndVerifyRepoDirPath(workDirId);
            var filePath = Path.Combine(repoDirPath, fileName);

            return !File.Exists(filePath) ? throw new ArgumentException($"The specified file \"{filePath}\" does not exist.") : filePath;
        }

        public string GetRepoDirPath(string workDirPath)
        {
            Argument.AssertNotEmpty(workDirPath, nameof(workDirPath));

            var childDirectories = Directory.GetDirectories(workDirPath, "*", SearchOption.TopDirectoryOnly);
            var repoDirectory = childDirectories.Single();

            return repoDirectory;
        }

        public string GetRepoDirName(string workDirPath)
        {
            Argument.AssertNotEmpty(workDirPath, nameof(workDirPath));

            var repoDirectory = this.GetRepoDirPath(workDirPath);
            var repoDirName = repoDirectory.Split('/', '\\').Last();

            return repoDirName;
        }

        public int DetermineCurrentRevisionNumber(string filePath, IReadOnlyList<FileRevisionInfo> revisionInfos)
        {
            Argument.AssertNotEmpty(filePath, nameof(filePath));
            Argument.AssertNotNull(revisionInfos, nameof(revisionInfos));

            var revisionInfo = revisionInfos.SingleOrDefault(r => r.FilePath == filePath);
            if (revisionInfo == null)
            {
                return 1;
            }

            return revisionInfo.CurrentRevisionNumber ?? 1;
        }

        public void IncrementRevisionNumber(string filePath, IList<FileRevisionInfo> revisionInfosToUpdate)
        {
            Argument.AssertNotEmpty(filePath, nameof(filePath));
            Argument.AssertNotNull(revisionInfosToUpdate, nameof(revisionInfosToUpdate));

            var revisionInfo = revisionInfosToUpdate.SingleOrDefault(r => r.FilePath == filePath);
            if (revisionInfo == null)
            {
                revisionInfosToUpdate.Add(new FileRevisionInfo
                {
                    FilePath = filePath,
                    CurrentRevisionNumber = 2,
                });
            }
            else
            {
                revisionInfo.CurrentRevisionNumber++;
            }
        }
    }
}
