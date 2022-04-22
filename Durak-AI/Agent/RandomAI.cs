using Model.PlayingCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.PlayingCards;
using Model.GameState;

namespace AIAgent
{
    public class RandomAI : Agent
    {
        private Random random = new Random();
        public RandomAI() { }

        public override Card? Move(GameView gameView)
        {
            List<Card>? cards = gameView.PossibleCards();

            return cards[random.Next(cards.Count)];
        }
    }
}
