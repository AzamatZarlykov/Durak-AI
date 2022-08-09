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

namespace CLI
{
    class Controller
    {
        private int[] gamesWon;

        private int draws;
        private int bouts;
        private double movesPerBout;

        public List<Agent> agents;

        private readonly GameParameters gParam;

        private readonly Wilson wilson_score;

        public Controller(GameParameters gameParam) 
        {
            Console.WriteLine($"Verbose: {gameParam.Verbose}, Debug: {gameParam.Debug}");
            this.gParam = gameParam;

            this.gamesWon = new int[2];
            this.agents = new List<Agent>();
            this.wilson_score = new Wilson();
        }

        private void DisplayWilsonScore(int total_games)
        {
            for (int i = 0; i < 2; ++i)
            {
                double win_proportion = (double)gamesWon[i] / gParam.NumberOfGames;
                (double, double) score = wilson_score.WilsonScore(
                    win_proportion, gParam.NumberOfGames);

                Console.WriteLine($"With 98% confidence, Agent {i + 1} ({agents[i].GetName()}) " +
                    $"wins between {(100 * score.Item1):f1}% and {(100 * score.Item2):f1}% ");
            }
            Console.WriteLine();
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
                DisplayWilsonScore(total_games);
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
            result = result == 1000 ? 0 : 1;

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

                    return new MinimaxAI($"{name} (depth={value})", value);
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
            Durak game = new Durak(gParam.StartingRank, gParam.Verbose, gParam.Debug);

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
    }
}
