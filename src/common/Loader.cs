using System.Text.Json;
using System.IO.Abstractions;
using System.Text.Json.Serialization;

namespace common;

public enum Language
{
    C,
    Cpp,
    CSharp,
    Java,
    Python,
    JavaScript,
    TypeScript,
    HTML,
    CSS,
    SQL,
    Ruby,
    PHP,
    Swift,
    Kotlin,
    Go,
    Rust,
    Scala,
    Perl,
    R,
    Shell,
    Other,
}

public class SessionEvents
{
    public Event[] Events { get; set; }
    public Language Language { get; set; }
    public int LinesOfCode { get; set; }
}

public class Loader
{
    private readonly IFileSystem _fileSystem;

    public Loader(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public Loader() : this(new FileSystem()) { }

    public Event[] Load(string path)
    {
        string text = _fileSystem.File.ReadAllText(path);
        Console.WriteLine("Read: " + text.Length + " bytes");
        text = text.Insert(0, "[");
        text = text.Remove(text.Length - 2, 2);
        text = text.Insert(text.Length, "]");

        // Fix messed up JSON
        text = text.Replace("\n", "\\n");
        text = text.Replace("},\\n{", "},\n{");

        Console.WriteLine("text has: " + text.Split("\n").Length + " lines");

        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter()},
            PropertyNameCaseInsensitive = true
        };
        Event[] events = JsonSerializer.Deserialize<Event[]>(text, options) ?? throw new InvalidOperationException("Failed to deserialize events");
        Console.WriteLine("Deserialized: " + events.Length + " events");
        return events;
    }
}
