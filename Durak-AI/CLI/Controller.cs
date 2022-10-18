using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AIAgent;
using Model.DurakWrapper;
using Model.PlayingCards;
using Model.GameState;
using Helpers.Wilson_Score;
using System.Globalization;

namespace CLI
{
    class Controller
    {
        private int[] gamesWon;

        private int draws;
        private int bouts;
        private double movesPerBout;

        public List<Agent> agents;

        private GameParameters gParam;

        private readonly Wilson wilson_score;

        public Controller(GameParameters gameParam) 
        {
            this.gParam = gameParam;

            this.gamesWon = new int[2];
            this.agents = new List<Agent>();
            this.wilson_score = new Wilson();
        }

        private (double, double)[] GetWilsonScore()
        {
            (double, double)[] results = new (double, double)[2];

            for (int i = 0; i < 2; ++i)
            {
                double win_proportion = (double)gamesWon[i] / (gParam.NumberOfGames - draws);
                results[i] = wilson_score.WilsonScore(
                    win_proportion, (gParam.NumberOfGames - draws));
            }
            return results;
        }

        private void DisplayWinRate(int total_games)
        {
            Console.WriteLine($"Draw rate: {(100 * (double)draws / total_games):f1}%");
            for (int i = 0; i < 2; ++i)
                Console.WriteLine($"Agent {i + 1} ({agents[i].GetName()}) won " +
                    $"{gamesWon[i]} / {total_games} games " +
                    $"({(100 * (double)gamesWon[i] / total_games):f1}%)");

            Console.WriteLine();
        }

        private void PrintStatistics()
        {
            int total_games = gParam.NumberOfGames;

            Console.WriteLine("\n==== STATISTICS ====");
            Console.WriteLine("Total games played: {0}\n", total_games);
            Console.WriteLine($"Average bouts played over the game: " +
                $"{((double)bouts / total_games):f1}");

            Console.WriteLine($"Average moves per bout over the game: " +
                $"{(movesPerBout / total_games):f1}\n");


            DisplayWinRate(total_games);

            if (total_games > 1)
            {
                var results = GetWilsonScore();

                for (int i = 0; i < results.Count(); i++)
                {
                    Console.WriteLine($"With 98% confidence, Agent {i + 1} ({agents[i].GetName()}) " +
                $"wins between {(100 * results[i].Item1):f1}% and {(100 * results[i].Item2):f1}% ");
                }
            }
        }
                                                                                                   
        private void HandleEndGameResult(Durak game, int gameIndex)
        {
            int bout = game.GetBoutsCount();
            int result = game.GetGameResult();
            double mpb = game.GetMovesPerBout();

            Console.Write("Game " + gameIndex + ": ");

            if (result == 0)
            {
                Console.Write("Draw");
                Console.WriteLine($" Total bouts: {bout}");
                draws++;
                return;
            }
            result = result == 1 ? 0 : 1;

            Console.Write($"Agent {result + 1} ({agents[result].GetName()}) won");
            Console.WriteLine($". Total bouts: {bout}");


            gamesWon[result]++;
            bouts += bout;
            movesPerBout += mpb; 
        }
        
        private Agent GetAgentType(string type, int param)
        {
            // for minimax:depth=3 OR montecarlo:depth=5
            string[] type_param = type.Split(':');
            string name = type_param[0];

            switch(name)
            {
                case "random":
                    return new RandomAI(name, param);
                case "greedy":
                    return new GreedyAI(name);
                case "smart":
                    return new Smart(name);
                case "minimax":
                    if (type_param.Count() == 1)
                    {
                        throw new Exception("Depth parameter is missing");
                    }
                    string[] res = type_param[1].Split('=');

                    if (res[0] != "depth")
                    {
                        Console.WriteLine();
                        throw new Exception($"Incorrect parameter name in {type_param[0]} AI: " +
                            $"{res[0]}");
                    }

                    if (res.Count() == 1)
                    {
                        throw new Exception("Depth value is missing");
                    }

                    int.TryParse(res[1], out int value);

                    if (value > 1000)
                    {
                        throw new ArgumentException(
                            $"Max depth value is 1000. The given value is {value}");
                    }

                    return new MinimaxAI($"{name} (depth={value})", value, gParam.D1, gParam.OpenWorld);
                default:
                    throw new Exception("unknown agent");
            }
        }

        private void InitializeAgents(int seed)
        {
            agents.Clear();

            string[] agentType = gParam.Agents;

            for (int i = 0; i < agentType.Length; i++)
            {
                agents.Add(GetAgentType(agentType[i], seed));
            }
        }

        public void Run()
        {
            Durak game = new Durak(
                gParam.StartingRank, 
                gParam.Verbose, 
                gParam.D2, 
                gParam.NoTrumpCards);

            int i = gParam.Seed == 0 ? 1 : gParam.Seed;
            int end = gParam.NumberOfGames == 1 ? i : gParam.NumberOfGames;

            Console.WriteLine("==== RUNNING ====\n");
            for (; i <= end; i++)
            {
                game.Initialize(i, gParam.Agents);
                InitializeAgents(i);

                while (game.gameStatus == GameStatus.GameInProcess)
                {
                    int turn = game.GetTurn();

                    Card? card = agents[turn].Move(new GameView(game, turn, gParam.OpenWorld));
                    game.Move(card);
                }
                HandleEndGameResult(game, i);
            }
            PrintStatistics();
        }

        private string GetTournamentConfig()
        {
            StringBuilder configs = new StringBuilder();

            configs.Append($"\"total_games: {gParam.NumberOfGames}, " +
                $"start_rank: {gParam.StartingRank}, ");
            if (gParam.OpenWorld)
            {
                configs.Append("open_world\",");
            }
            return configs.ToString();
        }

        private void GenerateCSV(Dictionary<string, string> results, string[] agents)
        {
            // initialize the table
            string[][] table = new string[agents.Length][];
            for (int i = 0; i < agents.Length; i++)
            {
                table[i] = new string[agents.Length];
            }
            // store the values
            int adder = 1;
            for (int i = 1; i < agents.Length; i++)
            {
                table[i][0] = agents[i];    // agents names in the first col
                table[0][i] = agents[i - 1];    // agents names in the first row
                for (int j = adder; j < agents.Length; j++)
                {
                    table[j][i] = results[$"{agents[j]}-{agents[i - 1]}"];
                }
                adder++;
            }

            using (var writer = new StreamWriter("tournament-results.csv"))
            {
                writer.WriteLine(GetTournamentConfig());

                for (int i = 0; i < table.Length; i++)
                {
                    for (int j = 0; j < table.Length; j++)
                    {
                        if (table[i][j] is null)
                        {
                            if (j == table.Length - 1) continue;
                            writer.Write(",");
                        }
                        else
                        {
                            string output = $"{table[i][j]}";
                            if (j != table.Length - 1)
                            {
                                output += ',';
                            }
                            writer.Write(output);
                        }
                    }
                    writer.WriteLine();
                }
            }
        }

        private void ResetProperties()
        {
            gamesWon = new int[2];
            draws = 0;
            bouts = 0;
            movesPerBout = 0;
        }

        public void RunTournament()
        {
            // random,greedy,smart,minimax:depth=5,minimax:depth=20
            string[] agents = gParam.TournamentAgents!.Split(',');
            // "smart-greedy": "38.1%-48.4%"
            Dictionary<string, string> results = new Dictionary<string, string>();

            int starter = 1;
            
            for (int i = 0; i < agents.Length - 1; i++)
            {
                for (int j = starter; j < agents.Length; j++)
                {
                    Console.WriteLine($"Game: {agents[j]} vs {agents[i]}");
                    gParam.Agents = new string[2] { agents[j], agents[i] };
                    ResetProperties();
                    Run();
                    var result = GetWilsonScore();
                    results.Add($"{agents[j]}-{agents[i]}",
                        $"{100 * result[0].Item1:f1}%-{100 * result[0].Item2:f1}%");
                }
                starter++;
            }
            GenerateCSV(results, agents);
        }
    }
}
