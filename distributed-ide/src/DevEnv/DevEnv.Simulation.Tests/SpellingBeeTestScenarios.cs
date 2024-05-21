using System.Text;

namespace DevEnv.Simulation.Tests
{
    /// <summary>
    /// Performs tests, which simulate predefined scenarios for data traffic meassurements, using the "SpellingBee" repository.
    /// </summary>
    public class SpellingBeeTestScenarios
    {
        [Fact]
        public async void SpellingBee_Build_Run_SimpleChange_Rebuild_Rerun()
        {
            using var serviceStarter = new ServiceStarter();
            using var mockEditor = new MockEditor();
            serviceStarter.StartServices();

            // Initialize a new work dir.
            var workDirId = await mockEditor.WorkDir.StartNewWorkDir("new work dir", "https://github.com/Tschouns/spelling-bee", "WIPRO-tests");

            var codeFiles = await mockEditor.WorkDir.GetFiles(workDirId);
            var solutionFile = codeFiles.Single(f => f.FileName.EndsWith(".sln"));

            // Build the solution a first time.
            var firstBuild = await mockEditor.Build.BuildDotnet(workDirId, solutionFile.FileName);
            Assert.True(firstBuild.IsSuccessful);

            var buildArtifacts = await mockEditor.Build.GetArtifactFiles(workDirId, firstBuild.BuildId);
            var exeFile = buildArtifacts.Single(f => f.FileName.EndsWith(".exe"));

            // Execute a first time.
            await this.Execute(mockEditor, workDirId, firstBuild.BuildId, exeFile.FileName);

            // Change a couple of files.
            var filesToChange = new[]
            {
                 "MustConsistOfRule.cs",
                 "MustContainRule.cs",
                 "MustHaveMinimumLengthRule.cs",
            };

            foreach (var codeFile in codeFiles.Where(f => filesToChange.Contains(f.FileName)))
            {
                // Download the file content.
                var fileBytes = await mockEditor.WorkDir.LoadFile(workDirId, codeFile.FileName);
                var fileText = Encoding.Unicode.GetString(fileBytes);

                // Remove an argument check (don't try this at home).
                var newFileText = fileText.Replace("Argument.AssertNotEmpty(word, nameof(word));", "");
                var newFileBytes = Encoding.Unicode.GetBytes(newFileText);

                // Update the file.
                await mockEditor.WorkDir.UpdateFile(workDirId, codeFile.FileName, newFileBytes);
            }

            // Build the solution a second time.
            var secondBuild = await mockEditor.Build.BuildDotnet(workDirId, solutionFile.FileName);
            Assert.True(secondBuild.IsSuccessful);

            // Execute a second time.
            await this.Execute(mockEditor, workDirId, secondBuild.BuildId, exeFile.FileName);
        }

        private async Task Execute(MockEditor mockEditor, string workDirId, string buildId, string exeFile)
        {
            // Start executing.
            var executionId = await mockEditor.Execute.StartExecuteDotnet(workDirId, buildId, exeFile);

            // Continuously read status and output; as a code editor might.
            var lineCount = 0;
            var status = await mockEditor.Execute.GetStatus(executionId);

            while (!status.HasExited && lineCount <= 20)
            {
                status = await mockEditor.Execute.GetStatus(executionId);
                var output = await mockEditor.Execute.GetOutputLines(executionId, offset: lineCount);
                lineCount += output.Count;

                Thread.Sleep(500);
            }

            // Stop the execution.
            await mockEditor.Execute.Kill(executionId);

            status = await mockEditor.Execute.GetStatus(executionId);
            Assert.True(status.HasExited);
        }
    }
}