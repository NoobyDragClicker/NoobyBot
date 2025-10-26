using System;
public static class Program
{
    public static void Main(string[] args)
    {
        Engine engine = new();

        if(args[0] == "bench")
        {
            engine.ReceiveCommand(args[0]);
        }

        string command = string.Empty;
        while (command != "quit")
        {
            command = Console.ReadLine();
            engine.ReceiveCommand(command);
        }
    }
}