namespace sim;

class MarkovChain {
    private State CurrentState;

    public MarkovChain(ILoadStates loader, string path) {
        CurrentState = loader.Load(path);
    }

    public void Step() {
        CurrentState = CurrentState.GetNextState();
    }
}
