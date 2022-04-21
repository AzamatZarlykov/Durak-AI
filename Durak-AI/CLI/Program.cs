using System;

using CLI.Parser;
using Model.DurakWrapper;

namespace CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            int numberOfGames = 10;

            for (int i = 1; i <= numberOfGames; i++)
            {
                Durak game = new Durak("randomAI", "randomAI");
            }
        }
    }
}


/*            Durak game = new Durak();
            ArgumentParser parser = new ArgumentParser(args);
            parser.Parse();

            if (parser.getSimulationType() == SimulationType.AiVSHuman)
            {
                if (parser.getFirstAIType() == AIType.Random)
                {

                }
            }*/