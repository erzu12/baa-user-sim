namespace sim;

using System.Threading.Tasks;
using GrpcBuild;

class Program
{
    private static async Task Main(string[] args)
    {
        int minSize = int.Parse(args.ElementAtOrDefault(0) ?? "20");
        int maxSize = int.Parse(args.ElementAtOrDefault(1) ?? "100");

        var ideService = new IDEService();
        var repoDir = ideService.GetWorkingDirectory() + "/QuestPDF";

        Console.WriteLine(repoDir);
        var repo = new GitRepo(repoDir);
        repo.Update("https://github.com/QuestPDF/QuestPDF.git");
        var Diffs = repo.FindCommitOfSize(minSize, maxSize);

        var loader = new LoadStates();
        var chain = new MarkovChain(loader, "chain.json");

        var IDE = new IDEService();

        Document[] docs = new Document[Diffs.Count];
        for (int diff = 0; diff < Diffs.Count; diff++)
        {
            docs[diff] = new Document(repoDir, Diffs[diff], IDE);
        }

        foreach (var doc in docs) {
            doc.RunEvents(chain);
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
}
