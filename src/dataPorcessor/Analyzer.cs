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

    public void AddTransition(EventName state, int count = 1)
    {
        if (!Transitions.ContainsKey(state))
        {
            Transitions.Add(state, 0);
        }
        Transitions[state] += count;
        TransitionCount += count;
    }
}


public class Analyzer
{
    public Dictionary<EventName, State> States { get; } = new Dictionary<EventName, State>();
    private EventName? _firstState;
    private State? _currentState;

    public void Analyze(Event[] events)
    {
        int totalAdded = 0;
        bool isAdjacient = false;
        int totalRemoved = 0;
        int totalSaved = 0;
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
                    if (e.EventName == EventName.DocumentChangeEvent) {
                        if (e.ChangeOperation == null) {
                            Console.WriteLine("ChangeOperation is null");
                            Console.WriteLine(e);
                        }
                        int changeCount = e.ChangeOperation.text.Length - int.Parse(e.ChangeOperation.rangeLength);
                        if (Math.Abs(changeCount) > 1000) { // very large changes skew the data
                            continue;
                        }
                        if (changeCount > 0) {
                            _currentState.AddTransition(EventName.AddCharcterEvent);
                            _currentState = SetState(EventName.AddCharcterEvent);
                            _currentState.AddTransition(EventName.AddCharcterEvent, changeCount - 1);
                            totalAdded += changeCount;
                        }
                        else if (changeCount < 0) {
                            _currentState.AddTransition(EventName.DeleteCharcterEvent);
                            _currentState = SetState(EventName.DeleteCharcterEvent);
                            _currentState.AddTransition(EventName.DeleteCharcterEvent, -changeCount - 1);
                            totalRemoved -= changeCount;
                        }
                    }
                    else {
                        _currentState.AddTransition(e.EventName);
                        _currentState = SetState(e.EventName);
                    }
                }
            }
        }

        Console.WriteLine("Total Added: " + totalAdded);
        Console.WriteLine("Total Removed: " + totalRemoved);
        Console.WriteLine("Total Saved: " + totalSaved);

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
