namespace sim;

class MarkovChain {
    private State CurrentState;

    public MarkovChain(ILoadStates loader) {
        CurrentState = loader.Load();
    }

    public void Step() {
        CurrentState = CurrentState.GetNextState();
    }
}
