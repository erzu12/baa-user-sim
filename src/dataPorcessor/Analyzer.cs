namespace dataProcessor;


public class State
{
    public EventName Name { get; set; }
    public Dictionary<EventName, int> Transitions { get; set; }
    public int TransitionCount { get; set; }

    public State(EventName name)
    {
        Name = name;
        Transitions = new Dictionary<EventName, int>();
        TransitionCount = 0;
    }

    public void AddTransition(EventName state)
    {
        if (!Transitions.ContainsKey(state))
        {
            Transitions.Add(state, 0);
        }
        Transitions[state]++;
        TransitionCount++;
    }
}


public class Analyzer
{
    public Dictionary<EventName, State> States { get; } = new Dictionary<EventName, State>();
    private State? _currentState;

    public void Analyze(Event[] events)
    {
        foreach (Event e in events)
        {
            if (_currentState == null)
            {
                _currentState = new State(e.EventName);
                States.Add(e.EventName, _currentState);
            }
            else
            {
                _currentState.AddTransition(e.EventName);
                _currentState = SetState(e.EventName);
            }
        }
    }

    private State SetState(EventName name)
    {
        if (!States.ContainsKey(name))
        {
            State state = new State(name);
            States.Add(name, state);
            return state;
        }
        else
        {
            return States[name];
        }
    }
}
