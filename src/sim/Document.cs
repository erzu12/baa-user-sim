using System.Text;
using common;
using GrpcBuild;

namespace sim;

class Document
{
    public string Path { get; set; }
    private string _repoDir;
    private string _repoDirName;
    public List<string> Content { get; set; }
    public FileDiff Diff { get; set; }
    private IIDEService _ideService;

    public Document(string repoDir, FileDiff diff, IIDEService ideService)
    {
        _ideService = ideService;
        _repoDir = repoDir;
        _repoDirName = repoDir.Split('/').Last();
        Path = repoDir + "/" + diff.File;
        Diff = diff;
        Console.WriteLine(Path);
        if (diff.IsAdded)
        {
            System.IO.File.WriteAllLines(Path, new string[0]);
        }
        var docBytes = _ideService.LoadFile(Path);
        Content = ContentFromBytes(docBytes);
    }

    public void RunSimEvents(MarkovChain chain, float addRatio)
    {
        int size = Diff.GetCharAddedCount();
        var events = chain.run(size, addRatio);
        int addEvents = 0;
        int deleteEvents = 0;
        int saveEvents = 0;
        while (true)
        {
            int currentRemoveLine = Diff.GetRemovedLine();
            if (currentRemoveLine < 0)
            {
                break;
            }
            Content.RemoveAt(currentRemoveLine);
        }
        FileDiff.Line? currentLine = Diff.GetAddedLine();
        Content.Insert(currentLine!.LineNumber, "");
        var rng = new Random();
        foreach (var e in events)
        {
            Console.WriteLine(e);
            if(e == EventName.AddCharcterEvent)
            {
                var c = currentLine?.GetNextChar();
                if (c == null) {
                    currentLine = Diff.GetAddedLine();
                    if (currentLine == null)
                    {
                        Console.WriteLine("No more lines");
                        break;
                    }
                    Content.Insert(currentLine!.LineNumber, "");
                    c = currentLine?.GetNextChar();
                }
                Content[currentLine!.LineNumber] += c;
                addEvents++;
            }
            if(e == EventName.DeleteCharcterEvent) {
                Console.WriteLine("Deleting");
                currentLine?.RemoveChar();
                if (Content[currentLine!.LineNumber].Length > 0)
                {
                    Content[currentLine!.LineNumber].Remove(Content[currentLine.LineNumber].Length - 1);
                }
                deleteEvents++;
            }
            if(e == EventName.DocumentSaveEvent)
            {
                //if(File.Exists(_repoDir + "/.git/index.lock"))
                //{
                    //Thread.Sleep(100);
                    //File.Delete(_repoDir + "/.git/index.lock");
                //}
                _ideService.UpdateFile(Path, ContentAsBytes());
                saveEvents++;
            }
            if(e == EventName.RunEvent) {
                //_ideService.Build(BuildSystem.Dotnet, "Source/QuestPDF.sln");
            }
        }

        Console.WriteLine($"Added: {addEvents}, Deleted: {deleteEvents}, Saved: {saveEvents}");
    }

    public void RunRealEvents(Event[] events)
    {
        Console.WriteLine(events.Last().EventTime);
        foreach (var e in events.Skip(1))
        {
            var eventPath = e.DocumentUri?.ToString().Split(new string[] { _repoDirName }, StringSplitOptions.None).Last() ?? "";
            if(!Path.EndsWith(eventPath))
            {
                continue;
            }
            Console.WriteLine(e.EventName + " " + e.Operation + " " + e.EventTime + "\nAdding: " + e.ChangeOperation?.text);
            if (e.EventName == EventName.DocumentChangeEvent)
            {
                if (e.ChangeOperation != null) {
                    var toAdd = e.ChangeOperation.text;
                    int startLine = int.Parse(e.ChangeOperation.RangeStart_Line);
                    int startChar = int.Parse(e.ChangeOperation.RangeStart_Character);
                    int endLine = int.Parse(e.ChangeOperation.RangeEnd_Line);
                    int endChar = int.Parse(e.ChangeOperation.RangeEnd_Character);
                    Console.WriteLine("oldline: " + Content[startLine]);

                    deleteInRange(startLine, startChar, endLine, endChar);
                    insertAt(startLine, startChar, toAdd);
                    Console.WriteLine("newLine: " + Content[startLine]);
                }
            }
            if (e.EventName == EventName.DocumentSaveEvent)
            {
                if(File.Exists(_repoDir + "/.git/index.lock"))
                {
                    Thread.Sleep(100);
                    File.Delete(_repoDir + "/.git/index.lock");
                }
                _ideService.UpdateFile(Path, ContentAsBytes());
            }
            //Console.ReadKey();
        }
    }

    private void deleteInRange(int startLine, int startChar, int endLine, int endChar)
    {
        Console.WriteLine(startLine + " " + startChar + " " + endLine + " " + endChar);
        Console.WriteLine(Content[startLine].Length);
        startChar = Math.Min(startChar, Content[startLine].Length);
        endChar = Math.Min(endChar, Content[endLine].Length);
        if (startLine == endLine)
        {
            Content[startLine] = Content[startLine].Remove(startChar, endChar - startChar);
        }
        else
        {
            Content[startLine] = Content[startLine].Remove(startChar);
            Content[startLine] += Content[endLine].Substring(endChar);
            Content.RemoveAt(endLine);
            for (int i = startLine + 1; i < endLine; i++)
            {
                Content.RemoveAt(startLine + 1);
            }
        }
    }

    private void removeLineIfEmpty(int line)
    {
        if (Content[line].Length == 0)
        {
            Content.RemoveAt(line);
        }
    }

    private void insertAt(int startLine, int startChar, string text)
    {
        startChar = Math.Min(startChar, Content[startLine].Length);
        var lines = text.Split('\n');
        if (lines.Length == 1)
        {
            Console.WriteLine("Inserting in line: " + text);
            Content[startLine] = Content[startLine].Insert(startChar, text);
        }
        else
        {
            Console.WriteLine("Inserting multi line: " + text);
            for (int i = 1; i < lines.Length - 1; i++)
            {
                Content.Insert(startLine + i, lines[i]);
            }
            var lastLine = lines.Last() + Content[startLine].Substring(startChar);
            Content.Insert(startLine + lines.Length - 1, lastLine);
            Content[startLine] = Content[startLine].Remove(startChar);
            Content[startLine] += lines[0];
        }
    }

    private byte[] ContentAsBytes()
    {
        return Encoding.UTF8.GetBytes(string.Join("\n", Content));
    }

    private List<string> ContentFromBytes(byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes).Split('\n').ToList();
    }

    public override string ToString()
    {
        int i = 0;
        string result = "";
        foreach (var line in Content)
        {
            result += i + line + "\n";
            i++;
        }
        return result;
    }

}
