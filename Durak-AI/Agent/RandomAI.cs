using Model.PlayingCards;
using Model.GameState;
using Model.DurakWrapper;

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
            if (gameView.turn == Turn.Defending && gameView.takes)
            {
                return null;
            }

            List<Card> cards = gameView.PossibleCards();

            // cannot attack/defend
            if (cards.Count == 0)
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
