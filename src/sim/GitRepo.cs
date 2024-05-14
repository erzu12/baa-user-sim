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
                Commands.Fetch(repo, "origin", new string[0], null, null);
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

    public void FindCommitOfSize(int minSize, int maxSize)
    {
        using (var repo = new Repository(_path))
        {
            foreach (var commit in repo.Commits.Take(10))
            {
                var summary = new GitDiff(_path, commit.Parents.First().Sha, commit.Sha).CreateSummary();
                Console.WriteLine($"Commit {commit.MessageShort} has {summary.LinesAdded} lines added and {summary.LinesDeleted} lines deleted");
                if (summary.LinesAdded >= minSize && summary.LinesAdded <= maxSize)
                {
                    var FileDiffs = new GitDiff(_path, commit.Parents.First().Sha, commit.Sha).Full();
                    foreach (var fileDiff in FileDiffs)
                    {
                        Console.WriteLine($"File: {fileDiff.File}");
                        foreach (var line in fileDiff.LinesAdded)
                        {
                            Console.WriteLine($"+{line.Key}: {line.Value.LineNumber}, {line.Value.Content}");
                        }
                        foreach (var line in fileDiff.LinesDeleted)
                        {
                            Console.WriteLine($"-{line.Key}: {line.Value.LineNumber}, {line.Value.Content}");
                        }
                    }
                    return;
                }
            }
        }
    }
}
