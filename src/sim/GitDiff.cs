namespace sim;

using System.Diagnostics;
using System.Text.RegularExpressions;

public class FileDiff
{
    public string File { get; set; }
    public Dictionary<int, Line> LinesAdded { get; set; }
    public Dictionary<int, Line> LinesDeleted { get; set; }

    public class Line
    {
        public int LineNumber { get; set; }
        public string Content { get; set; }

        public Line(int lineNumber, string content)
        {
            LineNumber = lineNumber;
            Content = content;
        }
    }

    public FileDiff(string file)
    {
        File = file;
        LinesAdded = new Dictionary<int, Line>();
        LinesDeleted = new Dictionary<int, Line>();

    }
}

class GitDiff
{
    private string _parentSha = "";
    private string _sha = "";
    private string _repoPath = "";

    public class Summery
    {
        public int LinesAdded { get; set; }
        public int LinesDeleted { get; set; }
    }


    public GitDiff(string repoPath, string parentSha, string sha)
    {
        _repoPath = repoPath;
        _parentSha = parentSha;
        _sha = sha;
    }

    public Summery CreateSummary()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = $"diff --stat {_parentSha} {_sha}",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _repoPath
        };

        using (Process process = Process.Start(startInfo))
        {
            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                var lastLine = output.Split('\n')[^2];
                var Summery = new Summery
                {
                    LinesAdded = int.Parse(lastLine.Split(',')[1].Split(' ')[1]),
                    LinesDeleted = int.Parse(lastLine.Split(',')[2].Split(' ')[1])
                };
                return Summery;
            }
        }
        return new Summery();
    }

    public List<FileDiff> Full()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = $"diff {_parentSha} {_sha} -U0",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = _repoPath
        };

        var fileDiffList = new List<FileDiff>();
        using (Process process = Process.Start(startInfo))
        {
            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();


                int additonsInBlock = 0;
                int deletionsInBlock = 0;
                int oldStartLine = 0;
                int newStartLine = 0;

                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("---")) {
                        var fileName = line.Split(' ')[1].Remove(0, 2);
                        fileDiffList.Add(new FileDiff(fileName));
                    }
                    if (line.StartsWith("@@"))
                    {
                        Console.WriteLine(line);
                        oldStartLine = int.Parse(Regex.Match(line, ".*-([0-9]*)").Groups[1].Value);
                        newStartLine = int.Parse(Regex.Match(line, ".*\\+([0-9]*)").Groups[1].Value);
                        additonsInBlock = deletionsInBlock = 0;
                    }
                    if (line.StartsWith("+") && !line.StartsWith("+++"))
                    {
                        Console.WriteLine(line);
                        fileDiffList[^1].LinesAdded.Add(additonsInBlock + newStartLine, new FileDiff.Line(additonsInBlock + oldStartLine, line.Remove(0, 1)));
                        additonsInBlock++;
                    }
                    if (line.StartsWith("-") && !line.StartsWith("---"))
                    {
                        fileDiffList[^1].LinesDeleted.Add(deletionsInBlock + newStartLine, new FileDiff.Line(deletionsInBlock + oldStartLine, line.Remove(0, 1)));
                        deletionsInBlock++;
                    }
                }
            }
        }
        return fileDiffList;
    }
}
