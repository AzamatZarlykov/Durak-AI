using Model.GameState;
using Model.PlayingCards;
using Model.DurakWrapper;

namespace AIAgent
{
    public class GreedyAI : Agent
    {
        public GreedyAI(string name) 
        {
            this.name = name;
        }
        public Card? GetCard(List<Card> possibleCards, GameView gw)
        {
            List<Card> noTrumpCards = Helper.GetCardsWithoutTrump(possibleCards, gw.trumpCard?.suit);

            if (noTrumpCards.Count == 0)
            {
                if (gw.isEarlyGame)
                {
                    // do not attack with trump card if there is no need
                    if (gw.turn == Turn.Attacking && gw.bout.GetAttackingCards().Count > 0)
                    {
                        return null;
                    }
                    // attack/defend o/w
                    return Helper.GetLowestRank(possibleCards);
                }
                else
                {
                    return Helper.GetLowestRank(possibleCards);
                }
            }
            return Helper.GetLowestRank(noTrumpCards);
        }

        public override Card? Move(GameView gameView)
        {
            List<Card?> cards = gameView.PossibleMoves(excludePass: true);

            // cannot attack/defend
            if (cards.Count == 1 && cards[0] is null)
            {
                return null;
            }
            return GetCard(cards!, gameView);
        }
    }
}