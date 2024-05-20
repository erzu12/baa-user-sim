namespace sim;

using LibGit2Sharp;

class GitRepo
{
    private readonly string _path;

    public GitRepo(string path)
    {
        _path = path;
    }

    public void Update(string url)
    {
        Console.WriteLine("Updating repository");
        if(Directory.Exists(_path))
        {
            using (var repo = new Repository(_path))
            {
                ResetHard();
                Commands.Fetch(repo, "origin", new string[0], null, null);
                Commands.Checkout(repo, repo.Branches["origin/main"], new CheckoutOptions() { CheckoutModifiers = CheckoutModifiers.Force });
            }
        }
        else
        {
            Repository.Clone(url, _path);
        }
        Console.WriteLine("done");
    }

    public void goToCommit(string commitSha)
    {
        using (var repo = new Repository(_path))
        {
            var commit = repo.Lookup<Commit>(commitSha);
            if (commit == null)
            {
                Console.WriteLine("Commit not found");
                return;
            }
            Commands.Checkout(repo, commit);
        }
    }

    public void ResetHard()
    {
        using (var repo = new Repository(_path))
        {
            Commands.Checkout(repo, repo.Head.Tip, new CheckoutOptions() { CheckoutModifiers = CheckoutModifiers.Force });
        }
    }

    public List<FileDiff> FindCommitOfSize(int minSize, int maxSize)
    {
        using (var repo = new Repository(_path))
        {
            foreach (var commit in repo.Commits.Skip(1).Take(30))
            {
                var summary = new GitDiff(_path, commit.Parents.First().Sha, commit.Sha).CreateSummary();
                Console.WriteLine($"Commit {commit.MessageShort} has {summary.LinesAdded} lines added and {summary.LinesDeleted} lines deleted");
                if (summary.LinesAdded >= minSize && summary.LinesAdded <= maxSize)
                {
                    string parentSha = commit.Parents.First().Sha;
                    var FileDiffs = new GitDiff(_path, parentSha, commit.Sha).Full();
                    goToCommit(parentSha);
                    return FileDiffs;
                }
            }
        }
        return new List<FileDiff>();
    }
}
