namespace sim;

using common;

class MarkovChain {
    public State? CurrentState { get; private set; }

    public MarkovChain(ILoadStates loader, string path) {
        CurrentState = loader.Load(path);
    }

    public IEnumerable<EventName> run(int CharsToAdd) {
        int charsAdded = 0;
        while (CharsToAdd > charsAdded && CurrentState != null) {
            CurrentState = CurrentState?.GetNextState();
            if (CurrentState?.Name == EventName.DocumentChangeEvent) {
                charsAdded += 1;
            }
            if (CurrentState != null) {
                yield return (EventName)CurrentState.Name;
            }
        }
        yield return EventName.DocumentSaveEvent;
        yield return EventName.GitEvent;
    }
}
