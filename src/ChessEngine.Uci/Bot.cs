using System;
public static class Program
{
    public static void Main(string[] args)
    {
        Engine engine = new();

        string command = string.Empty;
        while (command != "quit")
        {
            command = Console.ReadLine();
            engine.ReceiveCommand(command);
        }
    }
}