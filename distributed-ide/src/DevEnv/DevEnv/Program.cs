using System.Text;
using DevEnv.Base.Settings;
using DevEnv.Build.Client;
using DevEnv.Execute.Client;
using DevEnv.WorkDir.Client;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.ReadKey();

        // Work Dir
        Console.WriteLine("===== Create Work Dir =====");
        var workDirClientSettings = new DevEnv.WorkDir.Client.Settings { WorkDirServiceAddress = "https://localhost:7188" };
        var workDirService = new RemoteWorkDirService(new InMemorySettingsProvider<DevEnv.WorkDir.Client.ISettings>(() => workDirClientSettings));

        var workDirId = await workDirService.StartNewWorkDir("new work dir", "https://github.com/Tschouns/spelling-bee", "WIPRO-tests");
        Console.WriteLine("New work dir ID: " + workDirId);

        var sourceFiles = await workDirService.GetFiles(workDirId);
        Console.WriteLine(string.Join(Environment.NewLine, sourceFiles.Select(f => f.FileName)));

        // Build 1
        Console.WriteLine("===== Build 1 =====");
        var buildClientSettings = new DevEnv.Build.Client.Settings { BuildServiceAddress = "https://localhost:7288" };
        var buildService = new RemoteBuildService(new InMemorySettingsProvider<DevEnv.Build.Client.ISettings>(() => buildClientSettings));
        var build1Result = await buildService.BuildDotnet(workDirId, "SpellingBee\\SpellingBee.sln");

        Console.WriteLine("Build ID: " + build1Result.BuildId);
        Console.WriteLine(build1Result.Output);

        var artifactFiles = await buildService.GetArtifactFiles(workDirId, build1Result.BuildId);
        Console.WriteLine(string.Join(Environment.NewLine, artifactFiles.Select(f => f.FileName)));

        // Update File(s)
        Console.WriteLine("===== Update Files =====");
        var updatedreadmeFileBytes = Encoding.ASCII.GetBytes("Some new updated content...");
        await workDirService.UpdateFile(workDirId, "README.md", updatedreadmeFileBytes);

        var updatedWordsFileBytes = Encoding.ASCII.GetBytes("beef\r\nbefell\r\nbeflout\r\nbefool\r\nbefoul\r\nbeleft\r\nbluff");
        await workDirService.UpdateFile(workDirId, "SpellingBee\\SpellingBee.Console\\Data\\words_alpha.txt", updatedWordsFileBytes);

        await workDirService.CommitChanges(workDirId, "Some change.");

        // Build 2
        Console.WriteLine("===== Build 2 =====");
        var build2Result = await buildService.BuildDotnet(workDirId, "SpellingBee\\SpellingBee.sln");

        Console.WriteLine("Build ID: " + build2Result.BuildId);
        Console.WriteLine(build2Result.Output);

        // Execute
        Console.WriteLine("===== Execute =====");
        var executeClientSettings = new DevEnv.Execute.Client.Settings { ExecuteServiceAddress = "https://localhost:7388" };
        var executeService = new RemoteExecuteService(new InMemorySettingsProvider<DevEnv.Execute.Client.ISettings>(() => executeClientSettings));
        var executeId = await executeService.StartExecuteDotnet(workDirId, build2Result.BuildId, "Debug\\net6.0\\SpellingBee.Console.exe");

        Console.WriteLine("Execute ID: " + executeId);

        var linesOffset = 0;
        var status = await executeService.GetStatus(executeId);

        while (!status.HasExited && linesOffset < 10)
        {
            var output = await executeService.GetOutputLines(executeId, linesOffset);

            foreach (var line in output)
            {
                Console.WriteLine(line);
            }

            linesOffset += output.Count;
            Thread.Sleep(100);
            status = await executeService.GetStatus(executeId);
        }

        await executeService.Kill(executeId);

        var remainingOutput = await executeService.GetOutputLines(executeId, linesOffset);
        foreach (var line in remainingOutput)
        {
            Console.WriteLine(line);
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}