using System.Text;
using DevEnv.Base.FileSystem;
using DevEnv.Base.Settings;
using DevEnv.TestTools.Fakes;
using DevEnv.WorkDir.Service.Git;
using DevEnv.WorkDir.Service.Services;
using DevEnv.WorkDir.Service.WorkingDirectory;
using Google.Protobuf;

namespace DevEnv.WorkDir.Service.Tests.Services
{
    /// <summary>
    /// Performs integration tests for the <see cref="WorkDirService"/> API,...
    /// ...testing the entire stack of the application, including file system interactions etc..
    /// </summary>
    public class WorkDirServiceTests
    {
        [Fact]
        public async void StartNewWorkDir_GitIgnoreRepo_SuccessfullyClonesRepo()
        {
            // Arrange
            using var testDir = new TempDirectory();

            var candidate = SetupCandidate(new Settings
            {
                DataDirectoryPath = testDir.FullName,
            });

            var description = "test";
            var repoUrl = "https://github.com/solidsloth/dotnet-gitignore.git";
            var branch = "master";

            // Act
            var reply = await candidate.StartNewWorkDir(new GrpcWorkDir.StartNewWorkDirRequest
            {
                Description = description,
                RepoUrl = repoUrl,
                Branch = branch,
            },
            new FakeServerCallContext());

            // Assert
            Assert.NotNull(reply);

            var workingDir = Directory.GetDirectories(testDir.FullName).Single();
            Assert.EndsWith(reply.Id, workingDir);

            var repoDir = Directory.GetDirectories(workingDir).Single();
            Assert.EndsWith("dotnet-gitignore", repoDir);

            var files = Directory.GetFiles(workingDir, "", SearchOption.TopDirectoryOnly);
            var info = File.ReadAllText(files.Single());
            Assert.Contains(description, info);
            Assert.Contains(repoUrl, info);
            Assert.Contains(branch, info);
        }

        [Fact]
        public async void LoadFile_ReadMeFile_ReturnsFileContent()
        {
            // Arrange
            using var testDir = new TempDirectory();

            var candidate = SetupCandidate(new Settings
            {
                DataDirectoryPath = testDir.FullName,
            });

            var startNewWorkDirReply = await candidate.StartNewWorkDir(new GrpcWorkDir.StartNewWorkDirRequest
            {
                Description = "Load file...",
                RepoUrl = "https://github.com/solidsloth/dotnet-gitignore.git",
                Branch = "load-file-test",
            },
            new FakeServerCallContext());

            // Act
            var reply = await candidate.LoadFile(new GrpcWorkDir.LoadFileRequest
            {
                WorkDirId = startNewWorkDirReply.Id,
                FilePath = "README.md",
            },
            new FakeServerCallContext());

            // Assert
            Assert.NotNull(reply);

            var tempFile = Path.Combine(testDir.FullName, "temp.txt");
            File.WriteAllBytes(tempFile, reply.Content.ToByteArray());
            var tempFileText = File.ReadAllText(tempFile);
            Assert.Contains("dotnet-gitignore", tempFileText);
        }

        [Fact]
        public async void UpdateFile_ReadMeFile_UpdatesFileContent()
        {
            // Arrange
            using var testDir = new TempDirectory();

            var candidate = SetupCandidate(new Settings
            {
                DataDirectoryPath = testDir.FullName,
            });

            var startNewWorkDirReply = await candidate.StartNewWorkDir(new GrpcWorkDir.StartNewWorkDirRequest
            {
                Description = "Update test",
                RepoUrl = "https://github.com/solidsloth/dotnet-gitignore.git",
                Branch = "update-test",
            },
            new FakeServerCallContext());

            var expectedFileContent = "Some new content";
            var tempFile = Path.Combine(testDir.FullName, "temp.txt");
            File.WriteAllText(tempFile, expectedFileContent);
            var fileContent = File.ReadAllBytes(tempFile);

            var fileName = "README.md";

            // Act
            var reply = await candidate.UpdateFile(new GrpcWorkDir.UpdateFileRequest
            {
                WorkDirId = startNewWorkDirReply.Id,
                FilePath = fileName,
                UpdatedContent = ByteString.CopyFrom(fileContent),
            },
            new FakeServerCallContext());

            // Assert
            Assert.NotNull(reply);

            var updatedFilePath = Path.Combine(testDir.FullName, startNewWorkDirReply.Id, "dotnet-gitignore", fileName);
            var updatedFileContent = File.ReadAllText(updatedFilePath);
            Assert.Equal(expectedFileContent, updatedFileContent);
        }

        [Fact]
        public async void CommitChanges_UpdatedReadmeFile_ChangedAreCommitted()
        {
            // Arrange
            using var testDir = new TempDirectory();

            var candidate = SetupCandidate(new Settings
            {
                DataDirectoryPath = testDir.FullName,
            });

            var startNewWorkDirReply = await candidate.StartNewWorkDir(new GrpcWorkDir.StartNewWorkDirRequest
            {
                Description = "Commit test",
                RepoUrl = "https://github.com/solidsloth/dotnet-gitignore.git",
                Branch = "commit-test",
            },
            new FakeServerCallContext());

            var fileName = "README.md";
            var fileContent = Encoding.ASCII.GetBytes("Some arbitrary content...");

            _ = await candidate.UpdateFile(new GrpcWorkDir.UpdateFileRequest
            {
                WorkDirId = startNewWorkDirReply.Id,
                FilePath = fileName,
                UpdatedContent = ByteString.CopyFrom(fileContent),
            },
            new FakeServerCallContext());

            // Act
            var reply = candidate.CommitChanges(new GrpcWorkDir.CommitChangesRequest
            {
                WorkDirId = startNewWorkDirReply.Id,
                CommitMessage = "Changed README.md file.",
            },
            new FakeServerCallContext());

            // Assert
            Assert.NotNull(reply);

            // TODO: find way to assert the repo state...
        }

        private static WorkDirService SetupCandidate(ISettings settings)
        {
            var settingsProvider = new InMemorySettingsProvider<ISettings>(() => settings);
            var candidate = new WorkDirService(
                new FakeLogger<WorkDirService>(),
                settingsProvider,
                new WorkDirHelper(
                    settingsProvider,
                    new MetadataFileUtils()),
                new GitCommandHelper(new FakeLogger<GitCommandHelper>()));

            return candidate;
        }
    }
}