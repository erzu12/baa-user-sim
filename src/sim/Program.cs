namespace sim;

class Program
{
    static void Main(string[] args)
    {
        var loader = new LoadStates();
        var chain = new MarkovChain(loader, "chain.json");
        chain.run(10, 128, EventName.GitEvent);

    }
}
