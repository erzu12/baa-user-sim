namespace sim;

using System.Threading.Tasks;
using GrpcBuild;
using common;
using DevEnv.Base.Settings;
using DevEnv.WorkDir.Client;
using ISettings = DevEnv.WorkDir.Client.ISettings;

class Program
{
    private static async Task Main(string[] args)
    {
        //InMemorySettingsProvider<ISettings> settingsProvider = new InMemorySettingsProvider<ISettings>(() => {
                //var settings = new Settings();
                //settings.WorkDirServiceAddress = "http://localhost:5188";
                //return settings;
                //});
        //var workDirService = new RemoteWorkDirService(settingsProvider);
        //workDirService.StartNewWorkDir("test", "https://github.com/erzu12/neith_ecs.git", "main").Wait();
        //return;
        


        int minSize = int.Parse(args.ElementAtOrDefault(0) ?? "20");
        int maxSize = int.Parse(args.ElementAtOrDefault(1) ?? "100");

        var ideService = new IDEService("{219f04f8-0226-4147-be54-60fc789d0610}");
        var repoDir = ideService.GetWorkingDirectory() + "/feasibilitystudy";

        Console.WriteLine(repoDir);
        var repo = new GitRepo(repoDir);
        //repo.Update("https://github.com/QuestPDF/QuestPDF.git");
        var (Diffs, startCommitTime, endCommitTime) = repo.FindCommitOfSize(minSize, maxSize);

        var loader = new LoadStates();
        var chain = new MarkovChain(loader, "chain.json");

        var eventLoader = new Loader();
        var events = eventLoader.Load("telemetry.json");
        events = trimEvents(events, startCommitTime, endCommitTime);
        Console.WriteLine($"Start event: {events[0].EventName}");

        Document[] docs = new Document[Diffs.Count];
        for (int diff = 0; diff < Diffs.Count; diff++)
        {
            docs[diff] = new Document(repoDir, Diffs[diff], ideService);
        }

        foreach (var doc in docs) {
            //doc.RunSimEvents(chain);
            doc.RunRealEvents(events);
        }
        Thread.Sleep(1000);
        //repo.ResetHard();
        Thread.Sleep(1000);
        //ideService.Build(BuildSystem.Dotnet, "Source/QuestPDF.sln");
        PrintTotalDiffSize(Diffs);
    }

    private static void PrintTotalDiffSize(List<FileDiff> diffs)
    {
        int charTotal = 0;
        int linesTotal = 0;
        int linesRemoved = 0;
        foreach (var diff in diffs)
        {
            charTotal += diff.GetCharAddedCount();
            linesRemoved += diff.LinesDeleted.Count;
            linesTotal += diff.LinesAdded.Count;
        }
        Console.WriteLine($"Total characters added: {charTotal}, Total lines added: {linesTotal}, Total lines removed: {linesRemoved}");
    }

    private static Event[] trimEvents(Event[] events, DateTimeOffset start, DateTimeOffset endTime)
    {
        int startEvent = 0;
        int endEvent = 0;
        Console.WriteLine($"Events: {events.Length}");
        Console.WriteLine($"Start time: {start}, End time: {endTime}");
        for (int i = 0; i < events.Length; i++)
        {
            if (events[i].EventName == EventName.GitEvent && events[i].Operation == Operation.commit)
            {
                var time = DateTimeOffset.FromUnixTimeMilliseconds(events[i].EventTime);
                Console.WriteLine($"Event time: {time}");
                if(Math.Abs((time - start).TotalSeconds) < 100)
                {
                    startEvent = i;
                }
                if(Math.Abs((time - endTime).TotalSeconds) < 100)
                {
                    endEvent = i;
                }
            }
        }
        Console.WriteLine($"Start event: {events[startEvent].EventTime}, End event: {events[endEvent].EventTime}");
        return events[startEvent..endEvent];
    }
}
