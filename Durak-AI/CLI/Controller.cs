using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AIAgent;
using Model.DurakWrapper;
using Model.PlayingCards;

namespace CLI
{
    class Controller
    {
        private int numberOfGames;

        private Agent playerA;
        private Agent playerB;

        private Durak game;
        public Controller(int n, string a, string b, int rankStartingPoint) 
        {
            numberOfGames = n;
            playerA = GetAgentType(a);
            playerB = GetAgentType(b);
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
                List<Card>? cards = game.PossibleCards();

            }
        }
    }
}
