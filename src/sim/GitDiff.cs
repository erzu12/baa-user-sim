namespace sim;

using System.Diagnostics;
using System.Text.RegularExpressions;

public class FileDiff
{
    public string File { get; set; }
    public bool IsAdded { get; set; }
    public Dictionary<int, Line> LinesAdded { get; set; }
    public Dictionary<int, Line> LinesDeleted { get; set; }
    private int _currentRemovedLine = 0;
    private int _currentAddedLine = 0;

    public class Line
    {
        public int LineNumber { get; set; } = 0;
        public string Content { get; set; } = "";
        private int _currentChar = 0;

        public Line(int lineNumber, string content)
        {
            LineNumber = lineNumber;
            Content = content;
        }
        
        public char? GetNextChar()
        {
            if (_currentChar < Content.Length)
            {
                return Content[_currentChar++];
            }
            return null;
        }
    }

    public int GetCharAddedCount()
    {
        return LinesAdded.Sum(x => x.Value.Content.Length);
    }

    public FileDiff(string file, bool isAdded)
    {
        File = file;
        IsAdded = isAdded;
        LinesAdded = new Dictionary<int, Line>();
        LinesDeleted = new Dictionary<int, Line>();

    }

    private void updatedLineNumbers(int shift, int fromLine)
    {
        foreach (var line in LinesAdded.Where(x => x.Value.LineNumber > fromLine).OrderBy(x => x.Key))
        {
            line.Value.LineNumber += shift;
        }
        foreach (var line in LinesDeleted.Where(x => x.Value.LineNumber > fromLine).OrderBy(x => x.Key))
        {
            line.Value.LineNumber += shift;
        }
    }

    public Line? GetAddedLine()
    {
        foreach (var line in LinesAdded.OrderBy(x => x.Key))
        {
            if (line.Key > _currentAddedLine)
            {
                _currentAddedLine = line.Key;
                updatedLineNumbers(1, line.Value.LineNumber);
                return line.Value;
            }
        }
        return null;
    }

    public int GetRemovedLine()
    {
        foreach (var line in LinesDeleted.OrderBy(x => x.Key))
        {
            if (line.Key > _currentRemovedLine)
            {
                _currentRemovedLine = line.Key;
                updatedLineNumbers(-1, line.Value.LineNumber);
                return line.Value.LineNumber;
            }
        }
        return -1;
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
                var delStr = Regex.Match(lastLine, ".* ([0-9]*) deletions").Groups[1]?.Value;
                var addStr = Regex.Match(lastLine, ".* ([0-9]*) insertions").Groups[1]?.Value;
                int deletions = string.IsNullOrEmpty(delStr) ? 0 : int.Parse(delStr);
                int additions = string.IsNullOrEmpty(addStr) ? 0 : int.Parse(addStr);
                var Summery = new Summery
                {
                    LinesAdded = additions,
                    LinesDeleted = additions
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
                bool isAdded = false;

                var lines = output.Split('\n');
                foreach (var line in lines)
                {
                    if (line.StartsWith("---")) {
                        isAdded = line.Contains("dev/null");
                    }
                    if (line.StartsWith("+++")) {
                        var fileName = line.Split(' ')[1].Remove(0, 2);
                        fileDiffList.Add(new FileDiff(fileName, isAdded));
                    }
                    if (line.StartsWith("@@"))
                    {
                        Console.WriteLine(line);
                        oldStartLine = int.Parse(Regex.Match(line, ".*-([0-9]*)").Groups[1].Value) - 1;
                        newStartLine = int.Parse(Regex.Match(line, ".*\\+([0-9]*)").Groups[1].Value) - 1;
                        oldStartLine = oldStartLine < 0 ? 0 : oldStartLine;
                        newStartLine = newStartLine < 0 ? 0 : newStartLine;
                        additonsInBlock = deletionsInBlock = 0;
                    }
                    if (line.StartsWith("+") && !line.StartsWith("+++"))
                    {
                        Console.WriteLine(line);
                        fileDiffList[^1].LinesAdded.Add(additonsInBlock + newStartLine, new FileDiff.Line(oldStartLine, line.Remove(0, 1)));
                        additonsInBlock++;
                    }
                    if (line.StartsWith("-") && !line.StartsWith("---"))
                    {
                        Console.WriteLine(line);
                        fileDiffList[^1].LinesDeleted.Add(deletionsInBlock + oldStartLine, new FileDiff.Line(oldStartLine, line.Remove(0, 1)));
                        deletionsInBlock++;
                    }
                }
            }
        }
        return fileDiffList;
    }
}
