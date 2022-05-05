using Model.GameState;
using Model.PlayingCards;
using Model.DurakWrapper;

namespace AIAgent
{
    public class GreedyAI : Agent
    {
        public GreedyAI() 
        {
            this.name = "GreedyAI";
        }

        private List<Card> GetCardsWithoutTrump(List<Card> cards, Card trump) =>
            cards.Where(c => c.suit != trump.suit).ToList();

        private Card GetLowestRank(List<Card> cards) =>
            cards.MinBy(c => c.rank)!;

        // return true if deck is not empty, o/w false
        private bool EarlyGame(GameView gw)
        {
            return gw.deck.cardsLeft != 0;
        }

        // select lowest card that is not a trump. O/W pass/take
        private Card? GetCard(List<Card> cards, Card trump, GameView gw)
        {
            List<Card> noTrumpCards = GetCardsWithoutTrump(cards, trump);

            if (noTrumpCards.Count == 0)
            {
                if (EarlyGame(gw))
                {
                    // do not attack with trump card if there is no need
                    if (gw.turn == Turn.Attacking && gw.attackingCards.Count > 0)
                    {
                        return null;
                    }
                    // attack/defend o/w
                    return GetLowestRank(cards);
                }
                else
                {
                    return GetLowestRank(cards);
                }
            }
            return GetLowestRank(noTrumpCards);
        }

        public override Card? Move(GameView gameView)
        {

            List<Card> cards = gameView.PossibleCards();

            // cannot attack/defend
            if (cards.Count == 0)
            {
                return null;
            }

            return GetCard(cards, gameView.trumpCard, gameView);
        }
    }
}
