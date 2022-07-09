using Model.GameState;
using Model.PlayingCards;
using Model.DurakWrapper;

namespace AIAgent
{
    public class GreedyAI : Agent
    {
        public GreedyAI() 
        {
        }

        private List<Card> GetCardsWithoutTrump(List<Card> cards, Suit trump) =>
            cards.Where(c => c.suit != trump).ToList();

        private Card GetLowestRank(List<Card> cards) =>
            cards.MinBy(c => c.rank)!;

        private List<Card> GetOpponentCards(GameView gw)
        {
            List<Card> cardsInBout = gw.bout.GetEverything();
            List<Card> discardPile = gw.discardPile.GetCards();
            List<Card> agentHand = gw.playerHand;

            List<Card> cards = new List<Card>();

            for (int suit = 0; suit < 4; suit++)
            {
                for(int rank = gw.deck.GetRankStart(); rank < 15; rank++)
                {
                    Card c = new Card((Suit)suit, (Rank)rank);
                    if (!cardsInBout.Contains(c) && !agentHand.Contains(c) && 
                        !discardPile.Contains(c))
                    {
                        cards.Add(c);
                    }
                }
            }
            return cards;
        }

        private Card? AttackingStrategy(GameView gw, List<Card> oHand, List<Card> pHand,
            List<Card> noTrumpCards, List<Card> possibleCards)
        {
            List<Card> weaknesses = gw.GetWeaknesses(possibleCards, oHand);

/*            Console.Write("Weakness Cards: ");
            foreach (Card card in weaknesses)
            {
                Console.Write(card + " ");
            }
            Console.WriteLine();*/

            // if P has only one weakness there is a winning strategy
            if (weaknesses.Count() == 1)
            {
                if (pHand.Count() == 1)
                {
                    return pHand[0];
                }
                Card weakCard = weaknesses[0];

                if (noTrumpCards.Count == 1 && noTrumpCards.Contains(weakCard))
                {
                    return GetLowestRank(possibleCards.Where(c => c != weakCard).ToList());
                }
                return GetLowestRank(noTrumpCards.Where(c => c != weakCard).ToList());
            }
            // if by attacking a weakness card a defensive card will be in a 
            // non-weakness card of an attacker
            else if (weaknesses.Count() > 1)
            {
                List<Card> nonweakness = possibleCards.Where(
                    card => !weaknesses.Contains(card)).ToList();

                if (weaknesses.Count <= nonweakness.Count)
                {
                    Card? weakCard = gw.GetBadlyCoveredWeakness(oHand,
                            nonweakness, weaknesses);

                    if (weakCard == null)
                    {
                        return GetLowestRank(noTrumpCards);
                    }
                    return weakCard;
                }
            }

            return GetLowestRank(noTrumpCards);
        }

        private Card? DefendingStrategy(GameView gw, List<Card> oHand, List<Card> pHand,
            List<Card> noTrumpCards, List<Card> possibleCards)
        {
            foreach (Card card in possibleCards)
            {
                if (!oHand.Any(c => c.rank == card.rank))
                {
                    // Console.WriteLine("NO POSESSION: ");
                    return card;
                }
            }

            return GetLowestRank(noTrumpCards);
        }

        // select lowest card that is not a trump. O/W pass/take
        private Card? GetCard(List<Card> possibleCards, GameView gw)
        {
            List<Card> noTrumpCards = GetCardsWithoutTrump(possibleCards, gw.trumpSuit);

            if (gw.isEarlyGame)
            {
                // there are only trump cards
                if (noTrumpCards.Count == 0)
                {
                    if (gw.turn == Turn.Attacking && gw.bout.GetAttackingCards().Count() > 0)
                    {
                        // do not add a trump card if adding extra card
                        return null;
                    }
                    return GetLowestRank(possibleCards);
                }
            }
            else
            {
                if (noTrumpCards.Count() == 0)
                {
                    if (gw.takes)
                    {
                        return null;
                    }
                    return GetLowestRank(possibleCards);
                }

                List<Card> opponentCards = GetOpponentCards(gw);
                List<Card> playerCards = gw.playerHand;

                // stategy works if P attacking and O does not have any trump cards
                if (gw.turn == Turn.Attacking && !opponentCards.Exists(c => c.suit == gw.trumpSuit))
                {
                    return AttackingStrategy(gw, opponentCards, playerCards, noTrumpCards, 
                        possibleCards);
                }
                else if (gw.turn == Turn.Defending)
                {
                    return DefendingStrategy(gw, opponentCards, playerCards, noTrumpCards,
                        possibleCards);
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

            return GetCard(cards, gameView);
        }
    }
}