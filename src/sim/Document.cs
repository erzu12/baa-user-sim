using System.Text;

namespace sim;

class Document
{
    public string Path { get; set; }
    public List<string> Content { get; set; }
    public FileDiff Diff { get; set; }
    private IIDEService _ideService;

    public Document(string repoDir, FileDiff diff)
    {
        Path = repoDir + "/" + diff.File;
        Diff = diff;
        using (var reader = new StreamReader(Path))
        {
            var strContent = reader.ReadToEnd();
            Content = strContent.Split('\n').ToList();
        }
        Console.WriteLine(this);
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
            Console.WriteLine("Removing line " + currentRemoveLine);
            Content.RemoveAt(currentRemoveLine);
        }
        FileDiff.Line? currentLine = Diff.GetAddedLine();
        Content.Insert(currentLine!.LineNumber, "");
        foreach (var e in events)
        {
            //Console.WriteLine(e + ", " + currentLine?.LineNumber);
            if(e == EventName.DocumentChangeEvent)
            {
                var c = currentLine?.GetNextChar();
                if (c == null) {
                    currentLine = Diff.GetAddedLine();
                    Content.Insert(currentLine!.LineNumber, "");
                    c = currentLine?.GetNextChar();
                }
                Content[currentLine!.LineNumber] += c;
            }
            if(e == EventName.DocumentSaveEvent)
            {
                using (var writer = new StreamWriter(Path))
                {
                    foreach (var line in Content)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
        }
    }

    private byte[] ContentAsBytes()
    {
        return Encoding.UTF8.GetBytes(string.Join("\n", Content));
    }

    private void ContentFromBytes(byte[] bytes)
    {
        Content = Encoding.UTF8.GetString(bytes).Split('\n').ToList();
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
