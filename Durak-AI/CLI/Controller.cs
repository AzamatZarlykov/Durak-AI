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
        public Controller(GameParameters gameParam, IWriter writer) 
        {
            this.gameParameters = gameParam;
            this.writer = writer;
        }

        private void PrintStatistics()
        {
            writer.WriteLine("\nTotal games played: " + gameParameters.NumberOfGames);
            writer.WriteLine("Average bouts played over the game: " + bouts / gameParameters.NumberOfGames);
            writer.WriteLine("Average moves per bout over the game: " + movesPerBout / gameParameters.NumberOfGames);
            writer.WriteLine("Draw rate: " + (100 * (double)draws / gameParameters.NumberOfGames).ToString("0.#") + "%");
            writer.WriteLine(gameParameters.Agents[0].GetName() + " win rate: " + (100 * (double)gamesWonForPlayer0 / gameParameters.NumberOfGames).ToString("0.#") + "%");
            writer.WriteLine(gameParameters.Agents[1].GetName() + " win rate: " + (100 * (double)gamesWonForPlayer1 / gameParameters.NumberOfGames).ToString("0.#") + "%");
            
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
                "Player " + result + " (" + gameParameters.Agents[result].GetName() + ") won."
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
