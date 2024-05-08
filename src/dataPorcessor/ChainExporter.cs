using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dataProcessor;



class StateDTO
{
    public EventName Name { get; set; }
    public Dictionary<string, double> Transitions { get; set; }
}


public class ChainExporter
{
    IFileSystem _fileSystem;

    public ChainExporter()
    {
        _fileSystem = new FileSystem();
    }

    public ChainExporter(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }


    public void Export(Analyzer analyzer, string path)
    {
        var states = ConvertStateList(analyzer.States);
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter()},
        };
        var json = JsonSerializer.Serialize(states, options);
        _fileSystem.File.WriteAllText(path, json);
    }

    private Dictionary<EventName, StateDTO> ConvertStateList(Dictionary<EventName, State> states)
    {
        var stateDTOs = new Dictionary<EventName, StateDTO>();
        foreach (var state in states)
        {
            var stateDTO = new StateDTO();
            stateDTO.Name = state.Key;
            stateDTO.Transitions = new Dictionary<string, double>();
            foreach (var transition in state.Value.Transitions)
            {
                stateDTO.Transitions.Add(transition.Key.ToString(), (double)transition.Value / state.Value.TransitionCount);
            }
            stateDTOs.Add(state.Key, stateDTO);
        }
        return stateDTOs;
    }
}
