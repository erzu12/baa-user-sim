namespace dataProcessor;

using common;


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
    private EventName? _firstState;
    private State? _currentState;

    public void Analyze(Event[] events)
    {
        foreach (Event e in events)
        {
            if(e.EventName != EventName.StartUpEvent) {
                if (_currentState == null)
                {
                    _currentState = new State(e.EventName);
                    States.Add(e.EventName, _currentState);
                    _firstState = e.EventName;
                }
                else
                {
                    _currentState.AddTransition(e.EventName);
                    _currentState = SetState(e.EventName);
                }
            }
        }

        _currentState = SetState(EventName.StartUpEvent);
        _currentState.AddTransition(_firstState ?? EventName.StartUpEvent);
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
