namespace sim;

class Program
{
    private static void Main(string[] args)
    {
        var repo = new GitRepo("test_repo");
        repo.Update("https://github.com/microsoft/vscode.git");
        var Diffs = repo.FindCommitOfSize(20, 100);

        var loader = new LoadStates();
        var chain = new MarkovChain(loader, "chain.json");

        Console.WriteLine(Diffs.First().File);
        Document doc = new Document("test_repo", Diffs.First());
        doc.RunEvents(chain);
    }
}
