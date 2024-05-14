namespace sim;

class Program
{
    static void Main(string[] args)
    {
        //var loader = new LoadStates();
        //var chain = new MarkovChain(loader, "chain.json");
        //chain.run(10, 128, EventName.GitEvent);

        var repo = new GitRepo("test_repo");
        repo.Update("https://github.com/microsoft/vscode.git");
        repo.FindCommitOfSize(20, 100);

    }
}
