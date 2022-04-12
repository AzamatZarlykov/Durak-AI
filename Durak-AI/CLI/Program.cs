using System;

using CLI.Parser;

namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            ArgumentParser parser = new ArgumentParser(args);

            try
            {
                parser.Parse();

                Console.WriteLine(parser.getSimulationType());
                Console.WriteLine(parser.getAIType());

            } catch (ArgumentException e)
            {
                Console.WriteLine("{0}: {1}", e.GetType().Name, e.Message);
            }


        }
    }
}

// dotnet run "ai_1_name" [options for ai_1_name] "ai_2_name" [options for ai_2_name] [number of games] 
// dotnet run "ai_name" [options for ai_1_name] 