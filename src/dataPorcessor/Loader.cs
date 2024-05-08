using System.Text.Json;
using System.IO.Abstractions;
using System.Text.Json.Serialization;

namespace dataProcessor;

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
        text = text.Insert(0, "[");
        text = text.Remove(text.Length - 2, 2);
        text = text.Insert(text.Length, "]");

        // Fix messed up JSON
        text = text.Replace("\n", "\\n");
        text = text.Replace("},\\n{", "},\n{");

        Console.WriteLine(text);
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter()},
            PropertyNameCaseInsensitive = true
        };
        Event[] events = JsonSerializer.Deserialize<Event[]>(text, options) ?? throw new InvalidOperationException("Failed to deserialize events");
        return events;
    }
}
