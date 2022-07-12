using Model.PlayingCards;
using Model.GameState;
using Model.DurakWrapper;

namespace AIAgent
{
    public class RandomAI : Agent
    {
        private Random random;
        public RandomAI(string name, int seed) 
        {
            this.name = name;
            this.random = new Random(seed);
        }

        public override Card? Move(GameView gameView)
        {
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
            if (rn <= 20 && gameView.bout.GetAttackingCards().Count > 0)
            {
                return null;
            }

            return cards[random.Next(cards.Count)];
        }
    }
}
