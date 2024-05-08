using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sim;

class StateDTO
{
    public EventName Name { get; set; }
    public Dictionary<EventName, double> Transitions { get; set; }

    override public string ToString()
    {
        return $"StateDTOs = {{EventName: {Name }, Transitions: {Transitions}}}";
    }
}

public class LoadStates : ILoadStates
{
    IFileSystem _fileSystem;

    public LoadStates(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public LoadStates()
    {
        _fileSystem = new FileSystem();
    }

    public State Load(string path)
    {
        string json = _fileSystem.File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter()},
        };
        var StateDTOs = JsonSerializer.Deserialize<Dictionary<EventName, StateDTO>>(json, options)
                ?? throw new InvalidOperationException("Failed to deserialize states");

        try {
            return ConvertState(StateDTOs);
        } catch (KeyNotFoundException e) {
            throw new InvalidOperationException("Invalid state transition", e);
        }
    }

    // references all states by name and returns the start state
    private State ConvertState(Dictionary<EventName, StateDTO> StateDTOs)
    {
        var states = new Dictionary<EventName, State>();
        foreach (var stateDTO in StateDTOs)
        {
            states[stateDTO.Key] = new State(stateDTO.Key);
        }
        foreach (var stateDTO in StateDTOs)
        {
            var state = states[stateDTO.Key];
            foreach (var transition in stateDTO.Value.Transitions)
            {
                state.AddTransition(states[transition.Key], transition.Value);
            }
        }
        return states[EventName.StartUpEvent];
    }
}
