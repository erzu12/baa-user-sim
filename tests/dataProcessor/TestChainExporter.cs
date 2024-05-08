namespace tests;

using System.IO.Abstractions;
using dataProcessor;
using Moq;

public class TestChainExporter
{
    [Fact]
    public void TestExport()
    {
        var analyzer = new Analyzer();
        var events = new Event[] {
        };
        analyzer.Analyze(events);
        var mockFileSystem = new Mock<IFileSystem>();
        mockFileSystem.Setup(x => x.File.WriteAllText("test.json", "{\"StartUpEvent\":{\"Name\":\"StartUpEvent\",\"Transitions\":{\"StartUpEvent\":1}}}")).Verifiable();
        var exporter = new ChainExporter(mockFileSystem.Object);
        exporter.Export(analyzer, "test.json");
        mockFileSystem.Verify();
    }

    [Fact]
    public void TestExporTransitions()
    {
        var analyzer = new Analyzer();
        var events = new Event[] {
                new Event(EventName.DocumentOpenEvent),
                new Event(EventName.DocumentChangeEvent),
                new Event(EventName.DocumentChangeEvent),
                new Event(EventName.DocumentCloseEvent),
        };
        analyzer.Analyze(events);
        var mockFileSystem = new Mock<IFileSystem>();
        mockFileSystem.Setup(x => x.File.WriteAllText("test.json", "{\"DocumentOpenEvent\":{\"Name\":\"DocumentOpenEvent\",\"Transitions\":{\"DocumentChangeEvent\":1}},\"DocumentChangeEvent\":{\"Name\":\"DocumentChangeEvent\",\"Transitions\":{\"DocumentChangeEvent\":0.5,\"DocumentCloseEvent\":0.5}},\"DocumentCloseEvent\":{\"Name\":\"DocumentCloseEvent\",\"Transitions\":{}},\"StartUpEvent\":{\"Name\":\"StartUpEvent\",\"Transitions\":{\"DocumentOpenEvent\":1}}}")).Verifiable();
        var exporter = new ChainExporter(mockFileSystem.Object);
        exporter.Export(analyzer, "test.json");
        mockFileSystem.Verify();
    }
}
