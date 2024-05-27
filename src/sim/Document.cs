using System.Text;
using common;
using GrpcBuild;

namespace sim;

class Document
{
    public string Path { get; set; }
    private string _repoDirName;
    public List<string> Content { get; set; }
    public FileDiff Diff { get; set; }
    private IIDEService _ideService;

    public Document(string repoDir, FileDiff diff, IIDEService ideService)
    {
        _ideService = ideService;
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

    public void RunSimEvents(MarkovChain chain)
    {
        int size = Diff.GetCharAddedCount();
        var events = chain.run(size);
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
        foreach (var e in events)
        {
            Console.WriteLine(e);
            if(e == EventName.DocumentChangeEvent)
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
            }
            if(e == EventName.DocumentSaveEvent)
            {
                _ideService.UpdateFile(Path, ContentAsBytes());
            }
            if(e == EventName.RunEvent) {
                _ideService.Build(BuildSystem.Dotnet, "Source/QuestPDF.sln");
            }
        }
    }

    public void RunRealEvents(Event[] events)
    {
        foreach (var e in events.Skip(1))
        {
            var eventPath = e.DocumentUri?.ToString().Split(new string[] { _repoDirName }, StringSplitOptions.None).Last() ?? "";
            if(!Path.EndsWith(eventPath))
            {
                continue;
            }
            Console.WriteLine(e.EventName + " " + e.Operation + " " + e.EventTime + "\n" + e.ChangeOperation?.text);
            if (e.EventName == EventName.DocumentChangeEvent)
            {
                if (e.ChangeOperation != null) {
                    var toAdd = e.ChangeOperation.text;
                    int startLine = int.Parse(e.ChangeOperation.RangeStart_Line);
                    int startChar = int.Parse(e.ChangeOperation.RangeStart_Character);
                    int endLine = int.Parse(e.ChangeOperation.RangeEnd_Line);
                    int endChar = int.Parse(e.ChangeOperation.RangeEnd_Character);
                    Console.WriteLine(Content[startLine]);

                    deleteInRange(startLine, startChar, endLine, endChar);
                    insertAt(startLine, startChar, toAdd);
                }
            }
            if (e.EventName == EventName.DocumentSaveEvent)
            {
                _ideService.UpdateFile(Path, ContentAsBytes());
            }
        }
    }

    private void deleteInRange(int startLine, int startChar, int endLine, int endChar)
    {
        Console.WriteLine(startLine + " " + startChar + " " + endLine + " " + endChar);
        if (startLine == endLine)
        {
            Console.WriteLine(Content[startLine].Length);
            Content[startLine] = Content[startLine].Remove(startChar, endChar - startChar);
            removeLineIfEmpty(startLine);
        }
        else
        {
            Content[startLine] = Content[startLine].Remove(startChar);
            removeLineIfEmpty(startLine);
            Content[endLine] = Content[endLine].Remove(0, endChar);
            removeLineIfEmpty(endLine);
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
        var lines = text.Split('\n');
        if (lines.Length == 1)
        {
            Content[startLine] = Content[startLine].Insert(startChar, text);
        }
        else
        {
            Content[startLine] = Content[startLine].Insert(startChar, lines[0]);
            for (int i = 1; i < lines.Length; i++)
            {
                Content.Insert(startLine + i, lines[i]);
            }
            //Content[startLine + lines.Length - 1] = Content[startLine].Substring(startChar);
            //Content[startLine] = Content[startLine].Remove(startChar);
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
