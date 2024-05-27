using System.Text;
using GrpcBuild;

namespace sim;

class Document
{
    public string Path { get; set; }
    public List<string> Content { get; set; }
    public FileDiff Diff { get; set; }
    private IIDEService _ideService;

    public Document(string repoDir, FileDiff diff, IIDEService ideService)
    {
        _ideService = ideService;
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

    public void RunEvents(MarkovChain chain)
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
                using (var writer = new StreamWriter(Path))
                {
                    _ideService.UpdateFile(Path, ContentAsBytes());
                }
            }
            if(e == EventName.RunEvent) {
                _ideService.Build(BuildSystem.Dotnet, "Source/QuestPDF.sln");
            }
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
