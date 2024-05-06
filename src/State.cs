namespace sim;

using System.Collections.Generic;

public class State {
    private string Name { get; set; }
    private Dictionary<State, double> Transitions { get; set; }

    public State(string name) {
        Name = name;
        Transitions = new Dictionary<State, double>();
    }

    public void AddTransition(State state, double probability) {
        Transitions.Add(state, probability);
    }

    public State GetNextState() {
        double randomValue = new Random().NextDouble();
        double sum = 0;
        foreach (var transition in Transitions) {
            sum += transition.Value;
            if (randomValue <= sum) {
                return transition.Key;
            }
        }
        return Transitions.Keys.Last();
    }
}
