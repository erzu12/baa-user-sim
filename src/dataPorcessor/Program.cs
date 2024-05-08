namespace dataProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var loader = new Loader();
            var events = loader.Load("/home/jonas/.vscode-telemetry/telemetry.log");
            var analyzer = new Analyzer();
            analyzer.Analyze(events);
            var exporter = new ChainExporter();
            exporter.Export(analyzer, "chain.json");
        }
    }
}
