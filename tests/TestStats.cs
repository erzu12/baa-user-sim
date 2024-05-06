namespace tests;

using sim;

public class TestStats
{

    [Fact]
    public void testGetNextState()
    {
        State state = new State("A");
        State stateB = new State("B");
        state.AddTransition(stateB, 1);
        Assert.Equal(state.GetNextState(), stateB);
    }
}
