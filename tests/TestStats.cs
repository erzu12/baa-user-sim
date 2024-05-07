namespace tests;

using Moq;
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

    [Fact]
    public void testGetNextStateWithProb()
    {
        var mockRandom = new Mock<Random>();
        mockRandom.Setup(rand => rand.NextDouble()).Returns(0.4);
        State state = new State("A", mockRandom.Object);
        State stateB = new State("B");
        State stateC = new State("C");
        state.AddTransition(stateB, 0.5);
        state.AddTransition(stateC, 0.5);
        Assert.Equal(state.GetNextState(), stateB);
        mockRandom.Setup(rand => rand.NextDouble()).Returns(0.6);
        Assert.Equal(state.GetNextState(), stateC);
    }
}
