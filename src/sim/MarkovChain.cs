namespace sim;

class MarkovChain {
    public State? CurrentState { get; private set; }

    public MarkovChain(ILoadStates loader, string path) {
        CurrentState = loader.Load(path);
    }

    public void run(int minEvents, int maxEvents, EventName? endEvent) {
        while (CurrentState != null && maxEvents-- > 0 && (minEvents-- > 0 || CurrentState.Name != endEvent)) {
            CurrentState = CurrentState.GetNextState();
            Console.WriteLine(CurrentState?.Name);
        }
    }
}
