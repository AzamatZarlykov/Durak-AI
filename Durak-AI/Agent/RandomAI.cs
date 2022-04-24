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
        private Random random = new Random();
        public RandomAI() { }

        public override Card? Move(GameView gameView)
        {
            List<Card>? cards = gameView.PossibleCards();

            if (cards is null)
            {
                return null;
            }

            Console.WriteLine("Possible cards: ");
            foreach(Card card in cards)
            {
                Console.Write(card);
            }
            Console.WriteLine();

            return cards[random.Next(cards.Count)];
        }
    }
}
