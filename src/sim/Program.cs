namespace sim;

using System.Threading.Tasks;
using common;
using GrpcBuild;

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


        if (args.Length < 2)
        {
            Console.WriteLine("Usage: sim <operation> [args]");
            return;
        }

        string operation = args[0];
        float addRatio = 1;
        int minSize = 20;
        int maxSize = 100;
        string temlemetry = "telemetry.json";
        if (operation == "sim") {
            addRatio = float.Parse(args.ElementAtOrDefault(1) ?? "0");
            minSize = int.Parse(args.ElementAtOrDefault(2) ?? "20");
            maxSize = int.Parse(args.ElementAtOrDefault(3) ?? "100");
        }
        else if (operation == "replay") {
            temlemetry = args.ElementAtOrDefault(1) ?? "telemetry.json";
            minSize = int.Parse(args.ElementAtOrDefault(2) ?? "20");
            maxSize = int.Parse(args.ElementAtOrDefault(3) ?? "100");
        }
        else {
            Console.WriteLine("Invalid operation");
            return;
        }

        var ideService = new IDEService("{b051c1f8-89b7-4a2f-89da-5c49ae027ac4}");
        var repoDir = ideService.GetWorkingDirectory() + "/QuestPDF";

        Console.WriteLine(repoDir);
        var repo = new GitRepo(repoDir);
        //repo.Update("https://github.com/QuestPDF/QuestPDF.git");
        repo.ResetToMain();
        var (Diffs, startCommitTime, endCommitTime) = repo.FindCommitOfSize(minSize, maxSize);


        Document[] docs = new Document[Diffs.Count];
        for (int diff = 0; diff < Diffs.Count; diff++)
        {
            docs[diff] = new Document(repoDir, Diffs[diff], ideService);
        }

        if (operation == "sim") {
            var loader = new LoadStates();
            var chain = new MarkovChain(loader, "chain_b.json");

            foreach (var doc in docs) {
                doc.RunSimEvents(chain, addRatio);
            }
        }
        else if (operation == "replay") {
            var eventLoader = new Loader();
            var events = eventLoader.Load(temlemetry);
            events = trimEvents(events, startCommitTime, endCommitTime);

            foreach (var doc in docs) {
                doc.RunRealEvents(events);
            }
        }
        Thread.Sleep(1000);
        repo.ResetHard();
        Thread.Sleep(1000);
        ideService.Build(BuildSystem.Dotnet, "Source/QuestPDF.sln");
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
