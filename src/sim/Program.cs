namespace sim;

using System.Threading.Tasks;

class Program
{
    private static async Task Main(string[] args)
    {
        var repo = new GitRepo("test_repo");
        repo.Update("https://github.com/QuestPDF/QuestPDF.git");
        var Diffs = repo.FindCommitOfSize(20, 100);

        var loader = new LoadStates();
        var chain = new MarkovChain(loader, "chain.json");

        Console.WriteLine(Diffs.First().File);

        Document[] docs = new Document[Diffs.Count];
        for (int diff = 0; diff < Diffs.Count; diff++)
        {
            docs[diff] = new Document("test_repo", Diffs[diff]);
        }

        foreach (var doc in docs) {
            doc.RunEvents(chain);
        }
    }
}
