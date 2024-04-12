namespace CardConsole;

public static class Program
{
    private static void Main()
    {
        var client = new GameClient();
        client.Start();
    }
}