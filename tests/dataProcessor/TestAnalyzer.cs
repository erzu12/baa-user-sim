namespace tests;

using dataProcessor;

public class TestAnalyzer {
    [Fact]
    public void TestAnalyzeTransitionCount() {
        var events = new Event[] {
                new Event(EventName.DocumentOpenEvent),
                new Event(EventName.DocumentSaveEvent),
                new Event(EventName.DocumentSaveEvent),
                new Event(EventName.DocumentCloseEvent),
        };
        Analyzer analyzer = new Analyzer();
        analyzer.Analyze(events);
        Assert.Equal(1, analyzer.States[EventName.DocumentOpenEvent].TransitionCount);
        Assert.Equal(2, analyzer.States[EventName.DocumentSaveEvent].TransitionCount);
        Assert.Equal(0, analyzer.States[EventName.DocumentCloseEvent].TransitionCount);
    }

    [Fact]
    public void TestAnalyzeTransitions() {
        var events = new Event[] {
                new Event(EventName.DocumentOpenEvent),
                new Event(EventName.DocumentSaveEvent),
                new Event(EventName.DocumentSaveEvent),
                new Event(EventName.DocumentSaveEvent),
                new Event(EventName.DocumentCloseEvent),
        };
        Analyzer analyzer = new Analyzer();
        analyzer.Analyze(events);
        Assert.Equal(1, analyzer.States[EventName.DocumentOpenEvent].Transitions[EventName.DocumentSaveEvent]);
        Assert.Equal(2, analyzer.States[EventName.DocumentSaveEvent].Transitions[EventName.DocumentSaveEvent]);
        Assert.Equal(1, analyzer.States[EventName.DocumentSaveEvent].Transitions[EventName.DocumentCloseEvent]);
    }
}
