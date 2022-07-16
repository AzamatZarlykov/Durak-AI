using Model.GameState;
using Model.PlayingCards;
using Model.DurakWrapper;

namespace AIAgent
{
    public class GreedyAI : Agent
    {
        private readonly bool simple;
        public GreedyAI(string name) 
        {
            this.name = name;
        }
        
        public GreedyAI(string name, bool type)
        {
            this.name = name;
            this.simple = type;
        }

        private List<Card> GetCardsWithoutTrump(List<Card> cards, Suit trump) =>
            cards.Where(c => c.suit != trump).ToList();

        private Card GetLowestRank(List<Card> cards) =>
            cards.MinBy(c => c.rank)!;


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

        private Card? AttackingStrategy(GameView gw, List<Card> oHand, List<Card> pHand,
                List<Card> noTrumpCards, List<Card> possibleCards)
        {
            List<Rank> weaknesses = Helper.GetWeaknesses(possibleCards, oHand);

            // if P has only one weakness there is a winning strategy
            if (weaknesses.Count() == 1)
            {
                Rank weakRank = weaknesses[0];
                // if only weakrank cards left in my hand
                if (pHand.All(c => c.rank == weakRank))
                {
                    return pHand[0];
                }

                return GetLowestRank(pHand.Where(c => c.rank != weakRank).ToList());
            }
            if (weaknesses.Count > 1)
            {
                // Helper.PrintRanks("Weaknesses: ", weaknesses);

                // if more than one weakness - by attacking a weakness card a defensive card will be
                // in a non-weakness card of an attacker
                List<Card> nonweakness = pHand.Where(
                    card => !weaknesses.Contains(card.rank)).ToList();

                // Helper.PrintCards("Non Weaknesses: ", nonweakness);

                // Console.WriteLine("Non weakness Rank Size: " + Helper.GetNonWeaknessRankSize(nonweakness));
                if (weaknesses.Count <= Helper.GetNonWeaknessRankSize(nonweakness))
                {
                    Rank? weakRank = Helper.GetBadlyCoveredWeakness(gw, oHand,
                        nonweakness, weaknesses);

                    // Console.WriteLine("weak rank: " + weakRank);
                    if (weakRank == null)
                    {
                        return GetLowestRank(noTrumpCards);
                    }
                    return Helper.GetCardsOfTheSameRank(pHand, weakRank)[0];
                }
            }
            // Console.WriteLine("NOOOOOOOOOOO");
            return GetLowestRank(noTrumpCards);
        }

        private Card? DefendingStrategy(List<Card> oHand, List<Card> noTrumpCards)
        {
            foreach (Card card in noTrumpCards)
            {
                if (!oHand.Any(c => c.rank == card.rank))
                {
                    return card;
                }
            }

            return GetLowestRank(noTrumpCards);
        }

        private Card? CallStrategy(GameView gw, List<Card> possibleCards, List<Card> noTrumpCards)
        {
            List<Card> oHand = gw.GetOpponentCards();
            List<Card> pHand = gw.playerHand;

            // stategy works if P attacking and O does not have any trump cards
            if (gw.turn == Turn.Attacking && !oHand.Exists(c => c.suit == gw.trumpSuit))
            {
                return AttackingStrategy(gw, oHand, pHand, noTrumpCards,
                    possibleCards);
            }
            
            if (gw.turn == Turn.Defending)
            {
                return DefendingStrategy(oHand, noTrumpCards);
            }
            return GetLowestRank(noTrumpCards);
        }

        private Card? GetCard(List<Card> possibleCards, GameView gw)
        {
            List<Card> noTrumpCards = GetCardsWithoutTrump(possibleCards, gw.trumpSuit);

            if (gw.isEarlyGame)
            {
                if (noTrumpCards.Count == 0)    // if only trump cards
                {
                    if (gw.turn == Turn.Attacking)
                    {
                        if (gw.bout.GetAttackingCardsSize() > 0)
                        {
                            return null;
                        }
                        return GetLowestRank(possibleCards);
                    }
                    return GetLowestRank(possibleCards); 
                }

                if (gw.open)    // if open world use strategies
                {
                    if (gw.turn == Turn.Defending)
                    {
                        return DefendingStrategy(gw.GetOpponentCards(), noTrumpCards);
                    }
                }
                return GetLowestRank(noTrumpCards);
            }
            else    // late game - for open and closed world same rule
            {
                if (noTrumpCards.Count == 0)
                {
                    // in late game it is better to attack with lowest trump cards
                    if (gw.takes)
                    {
                        return null;
                    }
                    return GetLowestRank(possibleCards);
                }
                return CallStrategy(gw, possibleCards, noTrumpCards);
            }
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