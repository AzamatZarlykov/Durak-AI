using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.PlayingCards;
using Model.GameState;

namespace AIAgent
{
    public static class Helper
    {
        private static bool IsWeakness(List<Card> sameRankCards, List<Card> oHand)
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

        public static List<Card> GetCardsOfTheSameRank(List<Card> hand, Rank? cardRank)
            => hand.Where(c => c.rank == cardRank).ToList();

        // returns the weakness ranks
        public static List<Rank> GetWeaknesses(List<Card> hand, List<Card> opponentHand)
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
        private static bool CardsInNonweakness(List<Card> defense, List<Card> nonweakness)
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
        private static List<Card> GetDefensiveCards(GameView gw, List<Card> oHand, List<Card> cards)
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
        public static Rank? GetBadlyCoveredWeakness(GameView gw, List<Card> oHand,
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
        public static int GetNonWeaknessRankSize(List<Card> cards)
        {
            List<Rank> ranks = new List<Rank>();

            foreach (Card card in cards)
            {
                Rank r = card.rank;
                if (!ranks.Contains(r))
                {
                    ranks.Add(r);
                }
            }
            return ranks.Count;
        }

        public static void PrintCards(string title, List<Card> cards)
        {
            Console.Write(title);
            foreach(Card card in cards)
            {
                Console.Write(card + " ");
            }
            Console.WriteLine();
        }

        public static void PrintRanks(string title, List<Rank> ranks)
        {
            Console.Write(title);
            foreach (Rank rank in ranks)
            {
                Console.Write(rank + " ");
            }
            Console.WriteLine();
        }

        // returns null if defensive trump card has a high value
        // returns the card itself it is not high value
        // in the early game it is better not to give away the high value ranks(A,K,Q,J,10)
        // high value ranks can be any
        public static Card? EvaluateDefensiveTrumpCard(Card defenseCard)
        {
            return defenseCard.HighValueRank() ? defenseCard : null;
        }
    }
}
