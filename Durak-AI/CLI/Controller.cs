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

        private readonly GameParameters gameParameters;

        private readonly Wilson wilson_score;

        public Controller(GameParameters gameParam) 
        {
            this.gameParameters = gameParam;

            this.gamesWon = new int[2];
            this.agents = new List<Agent>();
            this.wilson_score = new Wilson();
        }

        private void DisplayWilsonScore(int total_games)
        {
            for (int i = 0; i < 2; ++i)
            {
                double win_proportion = (double)gamesWon[i] / gameParameters.NumberOfGames;
                (double, double) score = wilson_score.WilsonScore(
                    win_proportion, gameParameters.NumberOfGames);

                Console.WriteLine($"With 98% confidence, Agent {i + 1} ({gameParameters.Agents[i]}AI) " +
                    $"wins between {(100 * score.Item1):f1}% and {(100 * score.Item2):f1}% ");
            }
            Console.WriteLine();
        }

        private void DisplayWinRate(int total_games)
        {
            Console.WriteLine($"Draw rate: {(100 * (double)draws / total_games):f1}%");
            for (int i = 0; i < 2; ++i)
                Console.WriteLine($"Agent {i + 1} ({gameParameters.Agents[i]}AI) won " +
                    $"{gamesWon[i]} / {total_games} games " +
                    $"({(100 * (double)gamesWon[i] / total_games)}%)");

            Console.WriteLine();
        }

        private void PrintStatistics()
        {
            int total_games = gameParameters.NumberOfGames;

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
                                                                                                   
        private void HandleEndGameResult(Durak game)
        {
            int bout = game.GetBoutsCount();
            int result = game.GetGameResult();
            double mpb = game.GetMovesPerBout();


            if (result == 2)
            {
                Console.Write("Draw.");
                Console.WriteLine($" Bouts: {bout}, Moves per bout: {mpb:f1}");
                draws++;
                return;
            }
            Console.Write($"Agent {result + 1} ({gameParameters.Agents[result]}) won");
            Console.WriteLine($" Bouts: {bout}, Moves per bout: {mpb:f1}");

            if (result == 0)
            {
                gamesWon[0]++;
            }
            else
            {
                gamesWon[1]++;
            }
            bouts += bout;
            movesPerBout += mpb; 
        }

        private Agent GetAgentType(string type, int param)
        {
            switch(type)
            {
                case "random":
                    return new RandomAI(param);
                case "greedy":
                    return new GreedyAI();
                case "minimax":
                    return new MinimaxAI(gameParameters.Depth);
                default:
                    throw new Exception("unknown agent");
            }
        }

        private void InitializeAgents(int seed)
        {
            agents.Clear();

            string[] agentType = gameParameters.Agents;

            for (int i = 0; i < agentType.Length; i++)
            {
                agents.Add(GetAgentType(agentType[i], seed));
            }
        }

        public void Run()
        {
            Durak game = new Durak(gameParameters.StartingRank, gameParameters.Verbose);

            int i = gameParameters.Seed == 0 ? 1 : gameParameters.Seed;
            int end = gameParameters.NumberOfGames == 1 ? i : gameParameters.NumberOfGames;

            Console.WriteLine("==== RUNNING ====\n");
            for (; i <= end; i++)
            {
                game.Initialize(i);
                InitializeAgents(i);

                while (game.gameStatus == GameStatus.GameInProcess)
                {
                    int turn = game.GetTurn();

                    Card? card = agents[turn].Move(new GameView(game));
                    game.Move(card);
                }
                Console.Write("Game " + i + ": ");
                HandleEndGameResult(game);
            }
            PrintStatistics();
        }
    }
}
