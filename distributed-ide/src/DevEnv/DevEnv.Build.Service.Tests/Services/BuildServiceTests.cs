using System.IO.Compression;
using DevEnv.Base.Caching;
using DevEnv.Base.FileSystem;
using DevEnv.Base.Settings;
using DevEnv.Build.Service.BuildArtifacts;
using DevEnv.Build.Service.BuildSystems;
using DevEnv.Build.Service.BuildSystems.Dotnet;
using DevEnv.Build.Service.BuildSystems.Maven;
using DevEnv.Build.Service.Services;
using DevEnv.TestTools.Fakes;
using DevEnv.TestTools.TestData;
using DevEnv.WorkDir.Client;
using Moq;

namespace DevEnv.Build.Service.Tests.Services
{
    /// <summary>
    /// Performs integration tests for the <see cref="BuildService"/> API,...
    /// ...testing the entire stack of the application, including file system interactions etc.,...
    /// ...but mocking the <see cref="IWorkDirService"/> API.
    /// </summary>
    public class BuildServiceTests
    {
        [Fact]
        public async void Build_ExampleDotnetSolution_BuildsBinaries()
        {
            // Arrange
            using var testDir = new TempDirectory();
            var fakeWorkDir = PrepareFakeWorkDir(testDir.FullName, TestRepos.SpellingBee);
            var mocks = new MockRepository(MockBehavior.Strict);

            var candidate = SetupCandidate(
                new Settings
                {
                    DataDirectoryPath = testDir.FullName,
                },
                mocks,
                fakeWorkDir);

            var workDirId = "{88B20927-87E2-4C89-9344-55C549F78595}";

            // Act
            var reply = await candidate.Build(new GrpcBuild.BuildRequest
                {
                    WorkDirId = workDirId,
                    RootFilePath = "spelling-bee\\SpellingBee\\SpellingBee.sln",
                    BuildSystem = GrpcBuild.BuildSystem.Dotnet,
                },
                new FakeServerCallContext());

            // Assert
            Assert.NotNull(reply);
            Assert.Equal(GrpcBuild.BuildStatus.Successful, reply.BuildStatus);
            Assert.False(string.IsNullOrEmpty(reply.BuildOutput));
            
            var expectedArtifactsDirectoryPath = Path.Combine(testDir.FullName, workDirId, "artifacts", reply.BuildId);
            Assert.True(Directory.Exists(expectedArtifactsDirectoryPath));

            var binaryFiles = Directory.GetFiles(expectedArtifactsDirectoryPath, "*", SearchOption.AllDirectories);
            Assert.Equal(11, binaryFiles.Count());
            Assert.Contains(binaryFiles, f => f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async void Build_ExampleMavenProject_BuildsBinaries()
        {
            // Arrange
            using var testDir = new TempDirectory();
            var fakeWorkDir = PrepareFakeWorkDir(testDir.FullName, TestRepos.MavenExample);
            var mocks = new MockRepository(MockBehavior.Strict);

            var candidate = SetupCandidate(
                new Settings
                {
                    DataDirectoryPath = testDir.FullName,
                },
                mocks,
                fakeWorkDir);

            var workDirId = "{88B20927-87E2-4C89-9344-55C549F78595}";

            // Act
            var reply = await candidate.Build(new GrpcBuild.BuildRequest
            {
                WorkDirId = workDirId,
                RootFilePath = "maven-example\\pom.xml",
                BuildSystem = GrpcBuild.BuildSystem.Maven,
            },
            new FakeServerCallContext());

            // Assert
            Assert.NotNull(reply);
            Assert.Equal(GrpcBuild.BuildStatus.Successful, reply.BuildStatus);
            // Assert.False(string.IsNullOrEmpty(reply.BuildOutput)); -- TODO: output redirect does not work...

            var expectedArtifactsDirectoryPath = Path.Combine(testDir.FullName, workDirId, "artifacts", reply.BuildId);
            Assert.True(Directory.Exists(expectedArtifactsDirectoryPath));

            var binaryFiles = Directory.GetFiles(expectedArtifactsDirectoryPath, "*", SearchOption.AllDirectories);
            Assert.Equal(5, binaryFiles.Count());
            Assert.Contains(binaryFiles, f => f.EndsWith(".jar", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async void GetArtifactFiles_AfterDotnetBuild_ReturnsExpectedFilePaths()
        {
            // Arrange
            using var testDir = new TempDirectory();
            var fakeWorkDir = PrepareFakeWorkDir(testDir.FullName, TestRepos.SpellingBee);
            var mocks = new MockRepository(MockBehavior.Strict);

            var candidate = SetupCandidate(
                new Settings
                {
                    DataDirectoryPath = testDir.FullName,
                },
                mocks,
                fakeWorkDir);

            var workDirId = "{88B20927-87E2-4C89-9344-55C549F78595}";

            var buildReply = await candidate.Build(new GrpcBuild.BuildRequest
                {
                    WorkDirId = workDirId,
                    RootFilePath = "spelling-bee\\SpellingBee\\SpellingBee.sln",
                    BuildSystem = GrpcBuild.BuildSystem.Dotnet,
                },
                new FakeServerCallContext());

            // Act
            var reply = await candidate.GetArtifactFiles(new GrpcBuild.GetArtifactFilesRequest
                {
                    WorkDirId = workDirId,
                    BuildId = buildReply.BuildId,
                },
                new FakeServerCallContext());

            // Assert
            Assert.NotNull(reply);
            Assert.Equal(11, reply.Files.Count());
            Assert.All(reply.Files, f => Assert.False(Path.IsPathRooted(f.FilePath)));
            Assert.Contains("Debug\\net6.0\\SpellingBee.Console.exe", reply.Files.Select(f => f.FilePath));
        }

        [Fact]
        public async void LoadFile_ExeFile_ReturnsFileContent()
        {
            // Arrange
            using var testDir = new TempDirectory();
            var fakeWorkDir = PrepareFakeWorkDir(testDir.FullName, TestRepos.SpellingBee);
            var mocks = new MockRepository(MockBehavior.Strict);

            var candidate = SetupCandidate(
                new Settings
                {
                    DataDirectoryPath = testDir.FullName,
                },
                mocks,
                fakeWorkDir);

            var workDirId = "{88B20927-87E2-4C89-9344-55C549F78595}";

            var buildReply = await candidate.Build(new GrpcBuild.BuildRequest
                {
                    WorkDirId = workDirId,
                    RootFilePath = "spelling-bee\\SpellingBee\\SpellingBee.sln",
                    BuildSystem = GrpcBuild.BuildSystem.Dotnet,
                },
                new FakeServerCallContext());

            var filePath = "Debug\\net6.0\\SpellingBee.Console.exe";

            // Act
            var reply = await candidate.LoadArtifactFile(new GrpcBuild.LoadArtifactFileRequest
            {
                WorkDirId = workDirId,
                BuildId = buildReply.BuildId,
                FilePath = filePath,
            },
            new FakeServerCallContext());

            // Assert
            Assert.NotNull(reply);
            Assert.Equal(filePath, reply.FilePath);
        }

        private static string PrepareFakeWorkDir(string testDir, byte[] repoZip)
        {
            var repoZipFilePath = Path.Combine(testDir, "testrepo.zip");
            File.WriteAllBytes(repoZipFilePath, repoZip);

            var fakeWorkDir = Path.Combine(testDir, "fakeWorkDir");
            Directory.CreateDirectory(fakeWorkDir);
            ZipFile.ExtractToDirectory(repoZipFilePath, fakeWorkDir);

            return fakeWorkDir;
        }

        private static BuildService SetupCandidate(
            ISettings settings,
            MockRepository mocks,
            string fakeWorkDirPath)
        {
            var workDirFilePaths = Directory.GetFiles(fakeWorkDirPath, "*", SearchOption.AllDirectories);
            var workDirFileRevisions = workDirFilePaths
                .Select(f => Path.GetRelativePath(fakeWorkDirPath, f))
                .Select(f => new FileRevision(f, 1))
                .ToList();

            var workDirServiceMock = mocks.Create<IWorkDirService>();
            workDirServiceMock
                .Setup(m => m.GetFiles(It.IsAny<string>()))
                .Returns(Task.FromResult<IEnumerable<FileRevision>>(workDirFileRevisions));

            workDirServiceMock
                .Setup(m => m.LoadFile(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string workDirId, string fileName) =>
                {
                    var filePath = Path.Combine(fakeWorkDirPath, fileName);
                    return Task.FromResult(File.ReadAllBytes(filePath));
                });

            var candidate = new BuildService(
                new FakeLogger<BuildService>(),
                new BuildArtifactsHelper(
                    new InMemorySettingsProvider<ISettings>(() => settings),
                    new MetadataFileUtils()),
                workDirServiceMock.Object,
                new FileCacheManager(),
                new FileSystemUtils(),
                new BuildSystemProvider(
                    new DotnetBuildSystem(
                        new FakeLogger<DotnetBuildSystem>(),
                        new FileSystemUtils()),
                    new MavenBuildSystem(
                        new FakeLogger<MavenBuildSystem>(),
                        new FileSystemUtils())));

            return candidate;
        }
    }
}