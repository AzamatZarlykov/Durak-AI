using Model.GameState;
using Model.PlayingCards;
using Model.DurakWrapper;

namespace AIAgent
{
    public class GreedyAI : Agent
    {
        private readonly bool simple;
        private readonly bool open;
        public GreedyAI() 
        {
        }
        
        public GreedyAI(bool type, bool openWorld)
        {
            this.simple = type;
            this.open = openWorld;
        }

        private List<Card> GetCardsWithoutTrump(List<Card> cards, Suit trump) =>
            cards.Where(c => c.suit != trump).ToList();

        private Card GetLowestRank(List<Card> cards) =>
            cards.MinBy(c => c.rank)!;

        private List<Card> GetOpponentCards(GameView gw)
        {
            List<Card> cardsInBout = gw.bout.GetEverything();
            List<Card> discardPile = gw.discardPile;
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

        private bool IsWeakness(List<Card> sameRankCards, List<Card> oHand)
        {
            bool result = true;

            foreach (Card card in sameRankCards)
            {
                if (!oHand.Any(c => c.suit == card.suit && c.rank > card.rank))
                {
                    return false;
                }
            }

            return result;
        }

        private List<Card> GetCardsOfTheSameRank(List<Card> hand, Rank? cardRank)
            => hand.Where(c => c.rank == cardRank).ToList();

        // returns the weakness ranks
        public List<Rank> GetWeaknesses(List<Card> hand, List<Card> opponentHand)
        {
            List<Rank> visited = new List<Rank>();
            List<Rank> weaknesses = new List<Rank>();

            List<Card> sameType = new List<Card>();

            foreach (Card card in hand)
            {
                if (visited.Contains(card.rank))
                {
                    continue;
                }

                visited.Add(card.rank);
                sameType = GetCardsOfTheSameRank(hand, card.rank);
                
                if (IsWeakness(sameType, opponentHand))
                {
                    weaknesses.Add(card.rank);
                }
            }

            return weaknesses;
        }

        // checks if defensive cards are in the non weakness cards
        private bool CardsInNonweakness(List<Card> defense, List<Card> nonweakness)
        {
            bool result = true;


            foreach (Card card in defense)
            {
                if (!nonweakness.Any(c => c.rank == card.rank))
                {
                    return false;
                }
            }

            return result;
        }

        // returns the defensive cards that can defend all the attacking cards
        private List<Card> GetDefensiveCards(GameView gw, List<Card> oHand, List<Card> cards)
        {
            List<Card> result = new List<Card>();

            foreach (Card card in cards)
            {
                result.AddRange(oHand.Where(c => gw.IsLegalDefense(card, c)).ToList());
            }
            return result;
        }

        // This method returns the rank that is badly covered. Rank is badly covered
        // if by attacking the cards of that rank all the defending cards are within 
        // non weakness cards
        public Rank? GetBadlyCoveredWeakness(GameView gw, List<Card> oHand,
            List<Card> nws, List<Rank> ws)
        {
            Rank? weakCard = null;
            List<Card> sameRank = new List<Card>();

            foreach (Rank cardRank in ws)
            {
                sameRank = GetCardsOfTheSameRank(gw.playerHand, cardRank);

                List<Card> defensiveCards = GetDefensiveCards(gw, oHand, sameRank);

                if (CardsInNonweakness(defensiveCards, nws))
                {
                    return cardRank;

                }
            }
            return weakCard;
        }

        // This method returns the amout of ranks of the player cards
        // e.g cards: QH 10H QC JH -> 3 because (Q 10 J)
        private int GetNonWeaknessRankSize(List<Card> cards)
        {
            List<Rank> ranks = new List<Rank>();

            foreach(Card card in cards)
            {
                Rank r = card.rank;
                if (!ranks.Contains(r))
                {
                    ranks.Add(r);
                }
            }
            return ranks.Count;
        }

        private Card? AttackingStrategy(GameView gw, List<Card> oHand, List<Card> pHand,
            List<Card> noTrumpCards, List<Card> possibleCards)
        {
            List<Rank> weaknesses = GetWeaknesses(possibleCards, oHand);

            // if P has only one weakness there is a winning strategy
            if (weaknesses.Count() == 1)
            {
                if (pHand.Count() == 1)
                {
                    return pHand[0];
                }
                Rank weakRank = weaknesses[0];

                if (noTrumpCards.Count == 1 && noTrumpCards.Any(c => c.rank == weakRank))
                {
                    return GetLowestRank(possibleCards.Where(c => c.rank != weakRank).ToList());
                }
                return GetLowestRank(noTrumpCards.Where(c => c.rank != weakRank).ToList());
            }
            // if by attacking a weakness card a defensive card will be in a 
            // non-weakness card of an attacker
            else if (weaknesses.Count() > 1)
            {
                List<Card> nonweakness = possibleCards.Where(
                    card => !weaknesses.Contains(card.rank)).ToList();

                if (weaknesses.Count <= GetNonWeaknessRankSize(nonweakness))
                {
                    Rank? weakRank = GetBadlyCoveredWeakness(gw, oHand,
                            nonweakness, weaknesses);

                    if (weakRank == null)
                    {
                        return GetLowestRank(noTrumpCards);
                    }

                    return GetCardsOfTheSameRank(noTrumpCards, weakRank)[0];
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
                    return card;
                }
            }

            return GetLowestRank(noTrumpCards);
        }


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

                List<Card> opponentCards = open ? gw.opponentHand : GetOpponentCards(gw);
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

        public Card? GetCardSimple(List<Card> possibleCards, GameView gw)
        {
            List<Card> noTrumpCards = GetCardsWithoutTrump(possibleCards, gw.trumpSuit);

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
                    return GetLowestRank(possibleCards);
                }
                else
                {
                    return GetLowestRank(possibleCards);
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

            return simple ? GetCardSimple(cards, gameView) : GetCard(cards, gameView);
        }
    }
}