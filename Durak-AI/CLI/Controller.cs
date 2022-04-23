using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AIAgent;
using Model.DurakWrapper;
using Model.PlayingCards;
using Model.GameState;

namespace CLI
{
    class Controller
    {
        private int numberOfGames;

        private Durak game;
        
        private List<Agent> agents = new List<Agent>();

        private int numberOfTurns;
        private const int UPPER_BOUND = 1000;

        public Controller(int n, string a, string b, int rankStartingPoint) 
        {
            numberOfGames = n;
            agents.Add(GetAgentType(a));
            agents.Add(GetAgentType(b));
            game = new Durak(rankStartingPoint);
        }

        private Agent GetAgentType(string type)
        {
            if (type == "randomAI")
            {
                return new RandomAI();
            }
            return new RandomAI();
        }

        public void Run()
        {
            for (int i = 1; i <= numberOfGames; i++)
            {
                Console.WriteLine("Game: " + i);
                while (game.gameStatus == GameStatus.GameInProcess && numberOfTurns < UPPER_BOUND)
                {
                    int turn = game.GetTurn();

                    Card? card = agents[turn].Move(new GameView(game));
                    game.Move(card);
                    numberOfTurns++;
                }

            }
        }
    }
}
