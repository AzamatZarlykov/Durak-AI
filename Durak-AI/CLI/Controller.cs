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
        private const int UPPER_BOUND = 50_000;
        private const int GAME_INCREASE = 500;
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
                results[i].Item1 *= 100;
                results[i].Item2 *= 100;
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
                $"wins between {(results[i].Item1):f1}% and {(results[i].Item2):f1}% ");
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

        public void Run(bool tournament = false)
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
            // no need to print the stats for tournament games
            if (tournament)
            {
                return;
            }
            PrintStatistics();
        }

        private void WriteCSV(StreamWriter writer, List<string> eqAgents, string[][] table)
        {
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
            writer.WriteLine($"Equally Strong Agents (Upper bound reacher: {UPPER_BOUND}):");
            foreach(string agentsPair in eqAgents)
            {
                writer.WriteLine(agentsPair);
            }
        }

        private string GetFileName()
        {
            StringBuilder filename = new StringBuilder("tournament-results;");

            filename.Append($"rank={gParam.StartingRank},");
            filename.Append(gParam.OpenWorld ? "open_world.csv" : "closed_world.csv");

            return filename.ToString();
        }

        private void GenerateCSV(Dictionary<string, string> results, List<string> eqAgents, 
            string[] agents)
        {
            string dirPath = "CLI/Tournament";
            string fileName = GetFileName();
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

            try
            {
                Directory.CreateDirectory(dirPath);
                using (FileStream ostrm = new FileStream(Path.Combine(dirPath, fileName),
                    FileMode.Create, FileAccess.Write))
                using (StreamWriter writer = new StreamWriter(stream: ostrm))
                {
                    WriteCSV(writer, eqAgents, table);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

        private void ResetProperties(string[] agents, int i, int j, int setTotalGames)
        {
            gParam.Agents = new string[2] { agents[j], agents[i] };
            gParam.Seed = 1;
            gamesWon = new int[2];  // reset games won
            gParam.NumberOfGames = setTotalGames;   // reset the total games 
            draws = 0;
        }

        private bool IsResultSignificant((double, double) res) 
        {
            return res.Item1 > 50 || res.Item2 < 50;
        }
        
        public void RunTournament()
        {

            string[] agents = gParam.TournamentAgents!.Split(',');
            Dictionary<string, string> results = new Dictionary<string, string>();
            List<string> equallyStrongAgents = new List<string>();

            int starter = 1;
            int setTotalGames = gParam.NumberOfGames;
            (double, double)[] result;

            for (int i = 0; i < agents.Length - 1; i++)
            {
                for (int j = starter; j < agents.Length; j++)
                {
                    Console.WriteLine($"Game: {agents[j]} vs {agents[i]}");
                    ResetProperties(agents,i,j,setTotalGames);
                    while (true)
                    {
                        Run(true);
                        result = GetWilsonScore();
                        // significant result
                        if (IsResultSignificant(result[0]))
                        {
                            Console.WriteLine("Significant Enough");
                            Console.WriteLine($"{result[0].Item1:f1}%-{result[0].Item2:f1}%");
                            break;
                        }
                        else
                        {
                            Console.WriteLine($"Increasing the number of games and seed");
                            Console.WriteLine($"{result[0].Item1:f1}%-{result[0].Item2:f1}%");
                            Console.WriteLine($"Game: {agents[j]} vs {agents[i]}");
                            if (gParam.NumberOfGames >= UPPER_BOUND)
                            {
                                equallyStrongAgents.Add($"{agents[j]} - {agents[i]}");
                                break;
                            }
                            gParam.NumberOfGames += GAME_INCREASE;
                            gParam.Seed += GAME_INCREASE;
                        }
                    }

                    results.Add($"{agents[j]}-{agents[i]}",
                        $"{result[0].Item1:f1}%-{result[0].Item2:f1}%" +
                        $" ({gParam.NumberOfGames})");

                }
                starter++;
            }
            GenerateCSV(results, equallyStrongAgents, agents);
        }
    }
}
