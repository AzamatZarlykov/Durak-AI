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
        private int numberOfGamesWon;
        private readonly GameParameters gameParameters;
        private readonly IWriter writer;
        public Controller(GameParameters gameParam, IWriter writer) 
        {
            this.gameParameters = gameParam;
            this.writer = writer;
        }

        public void Run()
        {
            Durak game = new Durak(gameParameters.StartingRank, writer);

            for (int i = 1; i <= gameParameters.NumberOfGames; i++)
            {
                writer.Write("Game " + i + ": ");
                game.Initialize();
                while (game.gameStatus == GameStatus.GameInProcess)
                {
                    int turn = game.GetTurn();

                    Card? card = gameParameters.Agents[turn].Move(new GameView(game));
                    game.Move(card);
                }

                writer.WriteVerbose("GAME OVER!!!");

                int winner = game.GetWinner();
                writer.WriteLine("Player " + winner + " won");
                if (winner == 0)
                {
                    numberOfGamesWon++;
                }
            }

            writer.WriteLine("Total games played: " + gameParameters.NumberOfGames);
            writer.WriteLine("RandomAI win rate: " + 100 * (double)numberOfGamesWon/(double)gameParameters.NumberOfGames + "%");
        }
    }
}
