using DevEnv.Base.Caching;
using DevEnv.Base.FileSystem;
using DevEnv.Base.Processes;
using DevEnv.Base.Settings;
using DevEnv.Build.Client;
using DevEnv.Execute.Service.Execution;
using DevEnv.Execute.Service.Services;
using DevEnv.TestTools.Fakes;
using DevEnv.TestTools.TestData;
using Moq;
using System.IO.Compression;

namespace DevEnv.Execute.Service.Tests.Services
{
    /// <summary>
    /// Performs integration tests for the <see cref="ExecuteService"/> API,...
    /// ...testing the entire stack of the application, including file system interactions etc.,...
    /// ...but mocking the <see cref="IBuildService"/> API.
    /// </summary>
    public class ExecuteServiceTests
    {
        [Fact]
        public async void StartExecute_ExampleExe_ReturnsExecuteId()
        {
            // Arrange
            using var testDir = new TempDirectory();
            var fakeBuildDir = PrepareBuildArtifactsDir(testDir.FullName, TestArtifacts.SpellingBeeArtifacts);

            using var processManager = new ProcessManager();
            var mocks = new MockRepository(MockBehavior.Strict);

            var candidate = SetupCandidate(
                new Settings
                {
                    DataDirectoryPath = testDir.FullName ,
                },
                processManager,
                mocks,
                fakeBuildDir);

            var workDirId = "WORKDIRID";
            var buildId = "BUILDID";

            // Act
            var reply = await candidate.StartExecute(new GrpcExecute.StartExecuteRequest
                {
                    WorkDirId = workDirId,
                    BuildId = buildId,
                    Environment = GrpcExecute.Environment.DotnetExe,
                    RootFilePath = "Debug\\net6.0\\SpellingBee.Console.exe",
                },
                new FakeServerCallContext());

            // Assert
            Assert.NotNull(reply);

            var expectedArtifactsDownloadDirectoryPath = Path.Combine(testDir.FullName, workDirId);
            Assert.True(Directory.Exists(expectedArtifactsDownloadDirectoryPath));

            var downloadedaAtifactFiles = Directory.GetFiles(expectedArtifactsDownloadDirectoryPath, "*", SearchOption.AllDirectories);
            Assert.Equal(6, downloadedaAtifactFiles.Count());
        }

        [Fact]
        public async void StartExecute_ExampleJar_ReturnsExecuteId()
        {
            // Arrange
            using var testDir = new TempDirectory();
            var fakeBuildDir = PrepareBuildArtifactsDir(testDir.FullName, TestArtifacts.MavenExampleArtifacts);

            using var processManager = new ProcessManager();
            var mocks = new MockRepository(MockBehavior.Strict);

            var candidate = SetupCandidate(
                new Settings
                {
                    DataDirectoryPath = testDir.FullName,
                },
                processManager,
                mocks,
                fakeBuildDir);

            var workDirId = "WORKDIRID";
            var buildId = "BUILDID";

            // Act
            var reply = await candidate.StartExecute(new GrpcExecute.StartExecuteRequest
            {
                WorkDirId = workDirId,
                BuildId = buildId,
                Environment = GrpcExecute.Environment.JavaJar,
                RootFilePath = "maven-example-1.0-SNAPSHOT.jar",
            },
            new FakeServerCallContext());

            // Assert
            Assert.NotNull(reply);

            var expectedArtifactsDownloadDirectoryPath = Path.Combine(testDir.FullName, workDirId);
            Assert.True(Directory.Exists(expectedArtifactsDownloadDirectoryPath));

            var downloadedaAtifactFiles = Directory.GetFiles(expectedArtifactsDownloadDirectoryPath, "*", SearchOption.AllDirectories);
            Assert.Equal(7, downloadedaAtifactFiles.Count());
        }

        [Fact]
        public async void GetStatus_ExampleExecutable_ReturnsExpectedStatus()
        {
            // Arrange
            using var testDir = new TempDirectory();
            var fakeBuildDir = PrepareBuildArtifactsDir(testDir.FullName, TestArtifacts.SpellingBeeArtifacts);

            using var processManager = new ProcessManager();
            var mocks = new MockRepository(MockBehavior.Strict);

            var candidate = SetupCandidate(
                new Settings
                {
                    DataDirectoryPath = testDir.FullName,
                },
                processManager,
                mocks,
                fakeBuildDir);

            var workDirId = "WORKDIRID";
            var buildId = "BUILDID";

            var reply = await candidate.StartExecute(new GrpcExecute.StartExecuteRequest
                {
                    WorkDirId = workDirId,
                    BuildId = buildId,
                    Environment = GrpcExecute.Environment.DotnetExe,
                    RootFilePath = "Debug\\net6.0\\SpellingBee.Console.exe",
                },
                new FakeServerCallContext());

            // Act
            var status = await candidate.GetStatus(new GrpcExecute.GetStatusRequest { ExecuteId = reply.ExecuteId }, new FakeServerCallContext());

            // Assert
            Assert.False(status.HasExited);
        }

        [Fact]
        public async void Kill_WhileRunning_HasExitedWithExpectedExitCode()
        {
            // Arrange
            using var testDir = new TempDirectory();
            var fakeBuildDir = PrepareBuildArtifactsDir(testDir.FullName, TestArtifacts.SpellingBeeArtifacts);

            using var processManager = new ProcessManager();
            var mocks = new MockRepository(MockBehavior.Strict);

            var candidate = SetupCandidate(
                new Settings
                {
                    DataDirectoryPath = testDir.FullName,
                },
                processManager,
                mocks,
                fakeBuildDir);

            var workDirId = "WORKDIRID";
            var buildId = "BUILDID";

            var reply = await candidate.StartExecute(new GrpcExecute.StartExecuteRequest
            {
                WorkDirId = workDirId,
                BuildId = buildId,
                Environment = GrpcExecute.Environment.DotnetExe,
                RootFilePath = "Debug\\net6.0\\SpellingBee.Console.exe",
            },
                new FakeServerCallContext());

            // Act
            _ = await candidate.Kill(new GrpcExecute.KillRequest { ExecuteId = reply.ExecuteId }, new FakeServerCallContext());

            // Assert
            var status = await candidate.GetStatus(new GrpcExecute.GetStatusRequest { ExecuteId = reply.ExecuteId }, new FakeServerCallContext());
            Assert.True(status.HasExited);
            Assert.Equal(-1, status.ExitCode);
        }

        private static string PrepareBuildArtifactsDir(string testDir, byte[] artifactsZip)
        {
            var artifactsZipFilePath = Path.Combine(testDir, "artifacts.zip");
            File.WriteAllBytes(artifactsZipFilePath, artifactsZip);

            var fakeBuildDir = Path.Combine(testDir, "fakeBuildDir");
            Directory.CreateDirectory(fakeBuildDir);
            ZipFile.ExtractToDirectory(artifactsZipFilePath, fakeBuildDir);

            return fakeBuildDir;
        }

        private static ExecuteService SetupCandidate(
            ISettings settings,
            ProcessManager processManager,
            MockRepository mocks,
            string fakeBuildDirPath)
        {
            var artifactFilePaths = Directory.GetFiles(fakeBuildDirPath, "*", SearchOption.AllDirectories);
            var artifactFileRevisions = artifactFilePaths
                .Select(f => Path.GetRelativePath(fakeBuildDirPath, f))
                .Select(f => new FileRevision(f, 1))
                .ToList();

            var buildServiceMock = mocks.Create<IBuildService>();

            buildServiceMock
                .Setup(m => m.GetArtifactFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult<IEnumerable<FileRevision>>(artifactFileRevisions));

            buildServiceMock
                .Setup(m => m.LoadArtifactFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string workDirId, string buildId, string fileName) =>
                {
                    var filePath = Path.Combine(fakeBuildDirPath, fileName);
                    return Task.FromResult(File.ReadAllBytes(filePath));
                });

            var settingsProvider = new InMemorySettingsProvider<ISettings>(() => settings);
            var candidate = new ExecuteService(
                new FakeLogger<ExecuteService>(),
                settingsProvider,
                buildServiceMock.Object,
                new FileCacheManager(),
                new FileSystemUtils(),
                new ExecutorProvider(),
                processManager);

            return candidate;
        }
    }
}