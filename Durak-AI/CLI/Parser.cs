using System;
using System.Collections.Generic;
using CLAP;
using CLAP.Validation;

using AIAgent;
using Helpers.Writer;

namespace CLI
{
    public class Parser
    {
        private static void ExampleUsage()
        {
            Console.WriteLine("    usage example for multiple games: ");
            Console.WriteLine("\tdotnet run --project .\\CLI\\ -ai1=random -ai2=greedy -total_games=1000 -verbose=false");
            Console.WriteLine();

            Console.WriteLine("    usage example for a specific game: ");
            Console.WriteLine("\tdotnet run --project .\\CLI\\ -ai1=random -ai2=greedy -seed=29 -verbose=false");
            Console.WriteLine();
        }

        [Empty, Help]
        public static void Help(string help)
        {
            // this is an empty handler that prints
            // the automatic help string to the console.

            Console.WriteLine(help);
            ExampleUsage();
        }

        [Verb(IsDefault = true)]
        static void Parse(
            [DefaultValue("random")]
            [Description("The agent for player 1. Possible AIs: random, greedy, rule-based, " +
            "minimax, montecarlo")]
            string ai1,

            [DefaultValue("random")]
            [Description("The agent for player 2. Possible AIs: random, greedy, rule-based," +
            " minimax, montecarlo")]
            string ai2,

            [Description("A seed for random number generation")]
            int seed,

            [DefaultValue(1000)]
            [Description("The number of games to play")]
            int total_games,

            [DefaultValue(6)]
            [Description("The starting rank of cards in the deck")]
            int start_rank,

            [DefaultValue(false)]
            [Description("Make all cards visible to both players")]
            bool open_world,

            [DefaultValue(false)]
            [Description("Enable verbose output")]
            bool verbose
        )
        {
            if (seed > 0)
            {
                total_games = 1;
            }

            string[] agents = { ai1, ai2 };

            var gameParam = new GameParameters
            {
                NumberOfGames = total_games,
                StartingRank = start_rank,
                Agents = agents,
                Seed = seed,
                Verbose = verbose,
                OpenWorld = open_world,
            };

            Controller controller = new Controller(gameParam);
            controller.Run();
        }
    }
}
