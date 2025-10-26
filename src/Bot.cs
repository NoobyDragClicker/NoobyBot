using System;
public static class Program
{
    public static void Main(string[] args)
    {
        Engine engine = new();

        if(args.Length > 0 )
        {
            if(args[0] == "bench")
            {
                engine.ReceiveCommand(args[0]);
            }
        }
        else
        {
            string command = string.Empty;
            while (command != "quit")
            {
                command = Console.ReadLine();
                engine.ReceiveCommand(command);
            }
        }
    }
}