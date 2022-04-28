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

            // cannot attack/defend
            if (cards is null)
            {
                return null;
            }

            // include the case when the ai can decide to pass/take
            // even if it can defend/attack (20% chance)
            // allow only when the first attack was given
            int rn = random.Next(0, 100);
            if (rn <= 20 && gameView.attackingCards.Count > 0)
            {
                return null;
            }

            return cards[random.Next(cards.Count)];
        }
    }
}
