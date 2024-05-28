namespace sim;

using common;

class MarkovChain {
    public State? CurrentState { get; private set; }

    public MarkovChain(ILoadStates loader, string path) {
        CurrentState = loader.Load(path);
    }

    public IEnumerable<EventName> run(int CharsToAdd) {
        int charsAdded = 0;
        int additons = 1;
        int deletions = 1;
        float addRatio = 0.56f;
        var rng = new Random();
        while (CharsToAdd > charsAdded && CurrentState != null) {
            CurrentState = CurrentState?.GetNextState();
            if (CurrentState?.Name == EventName.AddCharcterEvent) {
                charsAdded += 1;
            }
            if (CurrentState?.Name == EventName.DeleteCharcterEvent) {
                var skipProb = addRatio - additons / (additons + deletions);
                if (rng.NextDouble() < skipProb) {
                    continue;
                }
                charsAdded -= 1;
            }
            Console.WriteLine(charsAdded + " of: " + CharsToAdd);
            if (CurrentState != null) {
                yield return (EventName)CurrentState.Name;
            }
        }
        yield return EventName.DocumentSaveEvent;
        yield return EventName.GitEvent;
    }
}
