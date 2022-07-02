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
            [DefaultValue("random"), Aliases("ai1")]
            [Description("A string parameter for a first AI name with an additional alias. " +
            "Possible AIs: random, greedy, minimax, montecarlo")]
            string ai_name_one,

            [DefaultValue("random"), Aliases("ai2")]
            [Description("A string parameter for a second AI name with an additional alias. " +
            "Possible AIs: random, greedy, minimax, montecarlo")]
            string ai_name_two,

            [Description("An int parameter that runs the specific game given indicated seed")]
            int seed,

            [DefaultValue(1000)]
            [Description("An int parameter to indicate total games to play")]
            int total_games,

            [DefaultValue(6)]
            [Description("An int parameter to indicate starting rank of the card")]
            int start_rank,

            [DefaultValue(false)]
            [Description("A bool parameter that allows to have all cards to be open or closed")]
            bool open_world,

            [DefaultValue(false)]
            [Description("A boolean parameter to indicate verbouse output")]
            bool verbose
        )
        {
            if (seed > 0)
            {
                total_games = 1;
            }

            string[] agents = { ai_name_one, ai_name_two };

            var gameParam = new GameParameters
            {
                NumberOfGames = total_games,
                StartingRank = start_rank,
                Agents = agents,
                Seed = seed
            };

            var writer = new Writer(Console.Out, verbose);

            Controller controller = new Controller(gameParam, writer);
            controller.Run();
        }
    }
}
