using System;

namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            int argsSize = args.Length;

            Console.WriteLine(argsSize);
            if (argsSize == 0)
            {
                // Task 1: Make a simple GUI for 2 player game

            }


        }
    }
}

// dotnet run "ai_1_name" [options for ai_1_name] "ai_2_name" [options for ai_2_name] [number of games] [logger]