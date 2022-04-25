using Model.PlayingCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.GameState;

namespace AIAgent
{
    public class RandomAI : Agent
    {
        private Random random;
        public RandomAI(int seed) 
        {
            this.random = new Random(seed);
            this.name = "RandomAI";
        }

        public override Card? Move(GameView gameView)
        {
            List<Card>? cards = gameView.PossibleCards();

            if (cards is null)
            {
                return null;
            }

            return cards[random.Next(cards.Count)];
        }
    }
}
