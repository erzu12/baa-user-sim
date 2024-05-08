namespace tests;

using Moq;
using sim;

public class TestStats
{

    [Fact]
    public void testGetNextState()
    {
        State state = new State(EventName.DocumentOpenEvent);
        State stateB = new State(EventName.DocumentChangeEvent);
        state.AddTransition(stateB, 1);
        Assert.Equal(state.GetNextState(), stateB);
    }

    [Fact]
    public void testGetNextStateWithProb()
    {
        var mockRandom = new Mock<Random>();
        mockRandom.Setup(rand => rand.NextDouble()).Returns(0.4);
        State state = new State(EventName.DocumentChangeEvent, mockRandom.Object);
        State stateB = new State(EventName.DocumentChangeEvent);
        State stateC = new State(EventName.DocumentChangeEvent);
        state.AddTransition(stateB, 0.5);
        state.AddTransition(stateC, 0.5);
        Assert.Equal(state.GetNextState(), stateB);
        mockRandom.Setup(rand => rand.NextDouble()).Returns(0.6);
        Assert.Equal(state.GetNextState(), stateC);
    }

    [Fact]
    public void testGetNextStateWithEmptyTransitions()
    {
        State state = new State(EventName.DocumentChangeEvent);
        Assert.Throws<InvalidOperationException>(() => state.GetNextState());
    }
}
