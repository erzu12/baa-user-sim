namespace sim;

using System.Collections.Generic;

public enum EventName
{
    DocumentOpenEvent,
    DocumentChangeEvent,
    DocumentCloseEvent,
    DocumentSaveEvent,
    StartUpEvent,
    GitEvent,
    RunEvent,
}


public class State {
    public EventName Name { get; }
    private Dictionary<State, double> Transitions;
    private Random RNG;

    public State(EventName name, Random? rng = default(Random)) {
        RNG = rng ?? new Random();
        Name = name;
        Transitions = new Dictionary<State, double>();
    }

    public void AddTransition(State state, double probability) {
        Transitions.Add(state, probability);
    }

    public State GetNextState() {
        double randomValue = RNG.NextDouble();
        double sum = 0;
        foreach (var transition in Transitions) {
            sum += transition.Value;
            if (randomValue <= sum) {
                return transition.Key;
            }
        }
        throw new InvalidOperationException("No transition found");
    }
}
