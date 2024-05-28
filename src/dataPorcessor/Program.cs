using common;

namespace dataProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var loader = new Loader();
            //var events = loader.Load("/home/jonas/.vscode-telemetry/telemetry.log");
            //var events = loader.Load("/media/ssd2/dev/HSLU/BAA/telData/telemetry.json");
            var events = loader.Load(args[0]);
            var analyzer = new Analyzer();
            analyzer.Analyze(events);
            var exporter = new ChainExporter();
            exporter.Export(analyzer, "chain_b.json");
        }
    }
}
