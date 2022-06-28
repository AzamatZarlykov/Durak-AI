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
        private int gamesWonForPlayer0;
        private int gamesWonForPlayer1;
        private int draws;
        private int bouts;
        private int movesPerBout;

        private readonly GameParameters gameParameters;

        private readonly IWriter writer;
        private readonly IWilson wilson_score;

        public Controller(GameParameters gameParam, IWriter writer) 
        {
            this.gameParameters = gameParam;
            this.writer = writer;
            this.wilson_score = new Wilson();
        }

        private void PrintStatistics()
        {
            writer.WriteLine("\nTotal games played: " + gameParameters.NumberOfGames);
            writer.WriteLine();
            writer.WriteLine("Average bouts played over the game: " + bouts / gameParameters.NumberOfGames);
            writer.WriteLine("Average moves per bout over the game: " + movesPerBout / gameParameters.NumberOfGames);
            writer.WriteLine();

            writer.WriteLine("Draw rate: " + (100 * (double)draws / gameParameters.NumberOfGames).ToString("0.#") + "%");
            writer.WriteLine();
            writer.WriteLine("Agent 0 (" + gameParameters.Agents[0].GetName() + ") win rate: " + (100 * (double)gamesWonForPlayer0 / gameParameters.NumberOfGames).ToString("0.#") + "%");
            writer.WriteLine("Agent 1 (" + gameParameters.Agents[1].GetName() + ") win rate: " + (100 * (double)gamesWonForPlayer1 / gameParameters.NumberOfGames).ToString("0.#") + "%");
            writer.WriteLine();

            double win_proportion0 = (double)gamesWonForPlayer0 / gameParameters.NumberOfGames;
            (double, double) score_0 = wilson_score.WilsonScore(win_proportion0, gameParameters.NumberOfGames);

            double win_proportion1 = (double)gamesWonForPlayer1 / gameParameters.NumberOfGames;
            (double, double) score_1 = wilson_score.WilsonScore(win_proportion1, gameParameters.NumberOfGames);

            Console.WriteLine("With 98% Confidence Interval, Agent 0 ({0}) wins between {1}% and {2}%",
                gameParameters.Agents[0].GetName(),
                (100 * score_0.Item1).ToString("0.#"),
                (100 * score_0.Item2).ToString("0.#")
                );

            Console.WriteLine("With 98% Confidence Interval, Agent 1 ({0}) wins between {1}% and {2}%",
                gameParameters.Agents[0].GetName(),
                (100 * score_1.Item1).ToString("0.#"),
                (100 * score_1.Item2).ToString("0.#")
                );
            writer.WriteLine();

        }

        private void HandleEndGameResult(Durak game)
        {
            writer.WriteLineVerbose("GAME OVER!!!");

            int bout = game.GetBoutsCount();
            int mpb = game.GetMovesPerBout();
            int result = game.GetGameResult();
            
            if (result == 2)
            {
                writer.Write("Draw.");
                writer.WriteLine(" Bouts: " + bout + ", Moves per bout: " + mpb);
                draws++;
                return;
            }

            writer.Write(
                "Agent " + result + " (" + gameParameters.Agents[result].GetName() + ") won."
            );

            writer.WriteLine(" Bouts: " + bout + ", Moves per bout: " + mpb);

            if (result == 0)
            {
                gamesWonForPlayer0++;
            }
            else
            {
                gamesWonForPlayer1++;
            }
            bouts += bout;
            movesPerBout += mpb;
        }

        public void Run()
        {
            Durak game = new Durak(gameParameters.StartingRank, gameParameters.Seed, writer);

            for (int i = 1; i <= gameParameters.NumberOfGames; i++)
            {
                writer.Write("Game " + i + ": ");
                game.Initialize();
                while (game.gameStatus == GameStatus.GameInProcess)
                {
                    int turn = game.GetTurn();
                    writer.WriteLineVerbose("TURN: " + turn);

                    Card? card = gameParameters.Agents[turn].Move(new GameView(game));
                    game.Move(card);
                }
                HandleEndGameResult(game);
            }
            PrintStatistics();
        }
    }
}
