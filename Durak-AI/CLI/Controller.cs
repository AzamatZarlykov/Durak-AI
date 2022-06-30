using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AIAgent;
using Model.DurakWrapper;
using Model.PlayingCards;
using Model.GameState;
using Helpers.Writer;
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

        private readonly IWriter writer;
        private readonly IWilson wilson_score;

        public Controller(GameParameters gameParam, IWriter writer) 
        {
            this.gameParameters = gameParam;
            this.writer = writer;

            this.gamesWon = new int[2];
            this.agents = new List<Agent>();
            this.wilson_score = new Wilson();
        }

        private void PrintStatistics()
        {
            int total_games = gameParameters.NumberOfGames;

            Console.WriteLine();
            Console.WriteLine("==== Statistics ====");
            Console.WriteLine("Total games played: {0}", total_games);
            Console.WriteLine();
            Console.WriteLine("Average bouts played over the game: {0}", 
                ((double)bouts / total_games).ToString("0.#"));

            Console.WriteLine("Average moves per bout over the game: {0}", 
                (movesPerBout / total_games).ToString("0.#"));
            Console.WriteLine();

            Console.WriteLine("Draw rate: {0}%",
                (100 * (double)draws / total_games).ToString("0.#"));
            Console.WriteLine();

            for (int i = 0; i < 2; ++i)
                Console.WriteLine("Agent {0} ({1}AI) win rate: {2}%", 
                    i + 1,
                    gameParameters.Agents[i], 
                    (100 * (double)gamesWon[i] / total_games).ToString("0.#")
                );
            
            Console.WriteLine();

            double win_proportion0 = (double)gamesWon[0] / total_games;
            (double, double) score_0 = wilson_score.WilsonScore(win_proportion0, total_games);

            double win_proportion1 = (double)gamesWon[1] / gameParameters.NumberOfGames;
            (double, double) score_1 = wilson_score.WilsonScore(win_proportion1, total_games);

            Console.WriteLine(
                "With 98% confidence, Agent 1 ({0}AI) wins between {1}% and {2}% (~ {3}-{4} games)",
                gameParameters.Agents[0],
                (100 * score_0.Item1).ToString("0.#"),
                (100 * score_0.Item2).ToString("0.#"),
                (int)(score_0.Item1 * total_games),
                (int)(score_0.Item2 * total_games)
                );

            Console.WriteLine(
                "With 98% confidence, Agent 2 ({0}AI) wins between {1}% and {2}% (~ {3}-{4} games)",
                gameParameters.Agents[1],
                (100 * score_1.Item1).ToString("0.#"),
                (100 * score_1.Item2).ToString("0.#"),
                (int)(score_1.Item1 * total_games),
                (int)(score_1.Item2 * total_games)
                );
            Console.WriteLine();
        }
                                                                                                   
        private void HandleEndGameResult(Durak game)
        {
            int bout = game.GetBoutsCount();
            int result = game.GetGameResult();
            double mpb = game.GetMovesPerBout();


            if (result == 2)
            {
                writer.Write("Draw.");
                writer.WriteLine(" Bouts: " + bout + ", Moves per bout: " + mpb.ToString("0.#"));
                draws++;
                return;
            }

            writer.Write(
                "Agent " + (result + 1) + " (" + gameParameters.Agents[result] + ") won."
            );

            writer.WriteLine(" Bouts: " + bout + ", Moves per bout: " + mpb.ToString("0.#"));

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
            Durak game = new Durak(gameParameters.StartingRank, writer);

            int i = gameParameters.Seed == 0 ? 1 : gameParameters.Seed;
            int end = gameParameters.NumberOfGames == 1 ? i : gameParameters.NumberOfGames;

            for (; i <= end; i++)
            {
                writer.Write("Game " + i + ": ");
                game.Initialize(i);
                InitializeAgents(i);

                while (game.gameStatus == GameStatus.GameInProcess)
                {
                    int turn = game.GetTurn();

                    Card? card = agents[turn].Move(new GameView(game));
                    game.Move(card);
                }
                HandleEndGameResult(game);
            }
            PrintStatistics();
        }
    }
}
