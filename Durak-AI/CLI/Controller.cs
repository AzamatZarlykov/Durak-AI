using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using AIAgent;
using Model.DurakWrapper;
using Model.PlayingCards;
using Model.GameState;
using Model.GamePlayer;
using Helpers.Wilson_Score;

namespace CLI
{
    class Controller
    {
        private int[] gamesWon;
        private int[] totalMoves;
        private Stopwatch[] timers;
        private int draws;
        private int bouts;
        private double movesPerBout;

        public List<Agent> agents;
        private GameParameters gParam;
        private readonly Wilson wilson_score;
        private const int UPPER_BOUND = 50_000;
        private const int GAME_INCREASE = 500;

        private StreamWriter? fwriter;
        public Controller(GameParameters gameParam) 
        {
            this.gParam = gameParam;

            this.gamesWon = new int[2];
            this.totalMoves = new int[2];
            this.timers = new Stopwatch[]
            {
                new Stopwatch(),
                new Stopwatch()
            };
            this.agents = new List<Agent>();
            this.wilson_score = new Wilson();

            if (gameParam.BF)
            {
                string dirpath = "ParamLogs";
                Directory.CreateDirectory(dirpath);
                FileStream fs = new FileStream(
                    Path.Combine(dirpath, "bf.txt"), FileMode.Append, FileAccess.Write);
                fwriter = new StreamWriter(stream: fs);
            }
        }

        ~Controller()
        {
            if (gParam.BF)
                fwriter!.Close();
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
            {
                // record only the first players win score
                if (gParam.Config && i == 0)
                {
                    // create a file to record the win results of the parameters of the first agent
                    string dirpath = "ParamLogs";
                    string filename = "result.txt";
                    Directory.CreateDirectory(dirpath);
                    using (FileStream fs = new FileStream(
                        Path.Combine(dirpath, filename), FileMode.Append, FileAccess.Write))
                    using (StreamWriter writer = new StreamWriter(stream: fs))
                    {
                        writer!.WriteLine($"{agents[i].GetName()}:{gamesWon[0]}:" +
                        $"{((double)timers[i].ElapsedMilliseconds / totalMoves[i]):f4}");
                    }
                }
                Console.WriteLine($"Agent {i + 1} ({agents[i].GetName()}) won " +
                    $"{gamesWon[i]} / {total_games} games " +
                    $"({(100 * (double)gamesWon[i] / total_games):f1}%)");
            }
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

            for (int i = 0; i < timers.Count(); i++)
            {
                Console.WriteLine($"Average time per move ({agents[i].name} agent): " +
                    $"{((double)timers[i].ElapsedMilliseconds / totalMoves[i]):f4}ms");
            }
            Console.WriteLine();


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
            Console.WriteLine();
        }
                                                                                                   
        private void HandleEndGameResult(Durak game, int gameIndex)
        {
            int bout = game.GetBoutsCount();
            int result = game.GetGameResult();
            double mpb = game.GetMovesPerBout();

            Console.Write("Game " + gameIndex + ": ");

            if (result == -1)
            {
                Console.Write("Draw");
                Console.WriteLine($" Total bouts: {bout}");
                draws++;
                return;
            }

            Console.Write($"Agent {result + 1} ({agents[result].GetName()}) won");
            Console.WriteLine($". Total bouts: {bout}");


            gamesWon[result]++;
            bouts += bout;
            movesPerBout += mpb;
        }


        private (int, int, string, string, double) ParseSearchParameters(string[] param)
        {
            // param = [mcts, limit=100,samples=20] || [minimax, depth=4,samples=20]
            if (param.Count() == 1)
            {
                throw new Exception("Parameters are missing");
            }

            //[ limit=100,samples=20,c=1.41 ] || [ depth=4,samples=20,eval=playout ] ||
            //[ depth=4,eval=playout ]
            string[] paramBuffer = param[1].Split(',');

            int value = 0;
            int samples = 20;
            double c = 1.41;
            bool success = false;
            string[] types = new string[2] { "random", "basic" };
            foreach (string p in paramBuffer)
            {
                string[] buffer = p.Split('=');

                if (buffer.Count() != 2)
                {
                    throw new Exception($"Parameter value is missing/wrong");
                }

                switch (buffer[0])
                {
                    case "limit":
                        success = int.TryParse(buffer[1], out value);
                        break;
                    case "depth":
                        success = int.TryParse(buffer[1], out value);
                        break;
                    case "samples":
                        success = int.TryParse(buffer[1], out samples);
                        break;
                    case "eval":
                        if (buffer[1] != "playout" && buffer[1] != "basic")
                        {
                            throw new Exception($"Wrong eval name: {buffer[1]}");
                        }
                        types[1] = buffer[1];
                        break;
                    case "simulation":
                        if (buffer[1] != "greedy" && buffer[1] != "random")
                        {
                            throw new Exception($"Wrong simulation name: {buffer[1]}");
                        }
                        types[0] = buffer[1];
                        break;
                    case "c":
                        success = double.TryParse(buffer[1], out c);
                        break;
                    default:
                        throw new Exception($"Wrong parameter name: { buffer[0] }");

                }
                if (!success)
                {
                    throw new Exception($"Invalid parameter for \"{buffer[0]}\": {buffer[1]}");
                }
            }
            return (value, samples, types[1], types[0], c);
        }
        
        private Agent GetAgentType(string type, int seed)
        {
            // -ai1=mcts:limit=100,samples=20 -ai2=minimax:depth=4,samples=20
            int samples;
            string[] type_param = type.Split(':');
            string name = type_param[0];
            string n;

            switch (name)
            {
                case "random":
                    return new RandomAI(name, seed);
                case "greedy":
                    return new GreedyAI(name);
                case "smart":
                    return new Smart(name);
                case "minimax":
                    int depth;
                    string evalType;

                    (depth, samples, evalType, _, _) = ParseSearchParameters(type_param);

                    n = gParam.OpenWorld ? $"{name}:depth={depth}:heuristic={evalType}" :
                        $"{name}:depth={depth}:heuristic={evalType}:samples={samples}";

                    return new MinimaxAI(n, depth, gParam.D1, evalType, samples);
                case "mcts":
                    int limit;
                    double c;
                    string simType;
                    (limit, samples, _, simType, c) = ParseSearchParameters(type_param);

                    n = gParam.OpenWorld ? $"{name}:iterations={limit}:C={c:f2}:simulation={simType}" :
                        $"{name}:iterations={limit}:C={c:f2}:simulation={simType}:samples={samples}";

                    Agent agent = simType == "random" ? new RandomAI("random", seed) :
                        new GreedyAI("greedy");

                    return new MCTS(n, limit, samples, c, agent);
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

        private void BranchingFactor(Durak game) 
        {
            var options = game.PossibleMoves(excludePass: true);

            fwriter!.Write($"{options.Count} ");
        }
        
        public void Run(bool tournament = false)
        {
            int i = gParam.Seed == 0 ? 1 : gParam.Seed;
            int end = gParam.NumberOfGames == 1 ? i : gParam.NumberOfGames;

            Console.WriteLine("\n==== RUNNING ====\n");
            for (; i <= end; i++)
            {
                Durak game = new Durak(
                    gParam.StartingRank,
                    gParam.Verbose,
                    gParam.D2,
                    gParam.IncludeTrumps,
                    gParam.OpenWorld,
                    i, gParam.Agents);

                InitializeAgents(i);

                while (game.gameStatus == GameStatus.GameInProcess)
                {
                    int turn = game.GetTurn();

                    totalMoves[turn]++;
                    timers[turn].Start();

                    var gw = new GameView(game, turn);

                    if (gParam.BF)
                        BranchingFactor(game);

                    Card? card = agents[turn].Move(gw);

                    if (!game.Move(card))
                        throw new Exception("Illegal Move");
                    
                    timers[turn].Stop();
                }
                HandleEndGameResult(game, i);
                if (gParam.BF)
                    fwriter!.WriteLine();
            }
            
            // no need to print the stats for tournament games
            if (tournament)
                return;
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

            if (eqAgents.Count > 0)
            {
                writer.WriteLine(new string(',', agents.Count));
                writer.WriteLine($"Equally Strong Agents (Upper bound: {UPPER_BOUND}):");
                foreach (string agentsPair in eqAgents)
                {
                    writer.WriteLine(agentsPair);
                }
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
            string[][] table = new string[agents.Length + 1][];
            for (int i = 0; i < agents.Length + 1; i++)
            {
                table[i] = new string[agents.Length + 1];
            }

            // store the values
            int adder = 1;
            for (int i = 1; i < agents.Length + 1; i++)
            {
                table[i][0] = agents[i - 1];    // agents names in the first col
                table[0][i] = agents[i - 1];    // agents names in the first row
                for (int j = adder; j < agents.Length + 1; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    table[j][i] = results[$"{agents[j - 1]}-{agents[i - 1]}"];
                    table[i][j] = results[$"{agents[i - 1]}-{agents[j - 1]}"];
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

                    results.Add($"{agents[i]}-{agents[j]}",
                        $"{result[1].Item1:f1}%-{result[1].Item2:f1}%" + 
                        $" ({gParam.NumberOfGames})");
                }
                starter++;
            }
            GenerateCSV(results, equallyStrongAgents, agents);
        }
    }
}
