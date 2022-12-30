using System;
using System.Collections.Generic;
using CLAP;
using CLAP.Validation;
using System.Text;

using AIAgent;
using Helpers;

namespace CLI
{
    public class Parser
    {
        private static void ExampleUsage()
        {
            string[] agents = new string[5]
            {
                "random", "greedy", "smart",
                "minimax:depth=<value>,samples=<value>,eval=<playout/basic>",
                "mcts:limit=<value>,c=<value>,simulation=<greedy/playout>,samples=<value>"
            };

            Console.WriteLine("\tPossible Agents:");
            foreach(string s in agents)
            {
                Console.WriteLine($"\t\t{s}");
            }
            Console.WriteLine();

            Console.WriteLine("\tusage example for multiple games: ");
            Console.WriteLine("\t\tdotnet run --project .\\CLI\\ -ai1=random -ai2=greedy " +
                "-total_games=1000 -verbose=true");
            Console.WriteLine();

            Console.WriteLine("\tusage example for a specific game: ");
            Console.WriteLine("\t\tdotnet run --project .\\CLI\\ -ai1=random -ai2=greedy -seed=29 " +
                "-verbose=false");
            Console.WriteLine();

            Console.WriteLine("\tusage example for a tournament between all the agents ");
            Console.WriteLine("\t\t$dotnet run --project .\\CLI\\ -tournament=\"random, greedy, smart, " +
                "minimax:depth=6, mcts:limit=100\" -total_games=500 -open_world -start_rank=10");
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


        private static void EnableLogs(Controller controller)
        {
            string dirPath = "CLI/GameLogs";
            string filename = "log.txt";
            try
            {
                Directory.CreateDirectory(dirPath);
                using (FileStream ostrm = new FileStream(Path.Combine(dirPath, filename),
                    FileMode.Append, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(stream:ostrm))
                {
                    Console.SetOut(writer);
                    controller.Run();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }

        }

        [Verb(IsDefault = true)]
        static void Parse(
            [DefaultValue("random")]
            [Description("The agent for player 1.")]
            string ai1,

            [DefaultValue("random")]
            [Description("The agent for player 2.")]
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
            bool verbose,

            [DefaultValue(false)]
            [Description("Displays the number of states and search depth for each minimax move")]
            bool d1,

            [DefaultValue(false)]
            [Description("Displays all the moves that minimax considers")]
            bool d2,

            [DefaultValue(false)]
            [Description("Enable logs for writing in the file")]
            bool log,

            [DefaultValue(true)]
            [Description("Enable trump cards in the game")]
            bool include_trumps,

            [Description("Runs the tournament with the agents specified. " +
            "E.g: -tournament=\"random,greedy,smart,minimax:depth=5,mcts:limit=100\"")]
            string tournament,

            [DefaultValue(false)]
            [Description("Used for grid search parameter configuration. For the purpose of experiments")]
            bool config,

            [DefaultValue(false)]
            [Description("Used for estimating the branching factor (with greedy agents)")]
            bool bf
        )
        {
            if (config && tournament is not null) { 
                throw new Exception($"Cannot run the tournament with 'config' parameter set");
            }

            if (tournament is not null)
            {
                Controller tournament_controller = new Controller(new GameParameters
                {
                    NumberOfGames = total_games,
                    StartingRank = start_rank,
                    Seed = 0,
                    Verbose = false,
                    D1 = false,
                    D2 = false,
                    OpenWorld = open_world,
                    IncludeTrumps = include_trumps,
                    TournamentAgents = tournament,
                });
                // No need for logs in tournaments; better to see the outputs on console
                tournament_controller.RunTournament();
                return;
            }
            if (seed > 0)
            {
                total_games = 1;
                verbose = true;
            }

            if (d2)
            {
                verbose = true;
            }

            string[] agents = { ai1, ai2 };

            if (ai1 != "greedy" && ai2 != "greedy" || total_games > 1)
            {
                throw new Exception($"-bf parameter works with -ai1=greedy, ai2=greedy and total_games=1");
            }

            var gameParam = new GameParameters
            {
                NumberOfGames = total_games,
                StartingRank = start_rank,
                Agents = agents,
                Seed = seed,
                Verbose = verbose,
                D1 = d1,
                D2 = d2,
                OpenWorld = open_world,
                IncludeTrumps = include_trumps,
                Config = config,
                BF = bf
            };

            Controller controller = new Controller(gameParam);

            if (!log)
            {
                controller.Run();
            }else
            {
                EnableLogs(controller);
            }

        }
    }
}
