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
        private int numberOfGamesWonForPlayer0;
        private int numberOfGamesWonForPlayer1;

        private readonly GameParameters gameParameters;
        private readonly IWriter writer;
        public Controller(GameParameters gameParam, IWriter writer) 
        {
            this.gameParameters = gameParam;
            this.writer = writer;
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

                writer.WriteLineVerbose("GAME OVER!!!");
                writer.WriteLineVerbose("Trump: " + game.GetTrumpCard());
                int winner = game.GetWinner();
                writer.WriteLine("Player " + winner + " won");
                if (winner == 0)
                {
                    numberOfGamesWonForPlayer0++;
                } else
                {
                    numberOfGamesWonForPlayer1++;
                }
            }

            writer.WriteLine("Total games played: " + gameParameters.NumberOfGames);
            writer.WriteLine(gameParameters.Agents[0].GetName() + " win rate: " + 100 * (double)numberOfGamesWonForPlayer0 / (double)gameParameters.NumberOfGames + "%");
            writer.WriteLine(gameParameters.Agents[1].GetName() + " win rate: " + 100 * (double)numberOfGamesWonForPlayer1 / (double)gameParameters.NumberOfGames + "%");

        }
    }
}
