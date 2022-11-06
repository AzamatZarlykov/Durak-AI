using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.GameState;
using Model.PlayingCards;
using Model.DurakWrapper;

namespace AIAgent
{
    public class Smart : Agent
    {
        // stores the cards of the opponents to use in strategies in the closed world
        private List<Card> memory = new List<Card>();
        public Smart(string name)
        {
            this.name = name;
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

                return Helper.GetLowestRank(pHand.Where(c => c.rank != weakRank).ToList());
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
                        return Helper.GetLowestRank(noTrumpCards);
                    }
                    return Helper.GetCardsOfTheSameRank(pHand, weakRank)[0];
                }
            }
            return Helper.GetLowestRank(noTrumpCards);
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

            return Helper.GetLowestRank(noTrumpCards);
        }

        private Card? CallStrategy(GameView gw, List<Card> possibleCards, List<Card> noTrumpCards)
        {
            // based on the env, GetOpponentCards() will return the cards
            List<Card> oHand = gw.GetOpponentCards();
            List<Card> pHand = gw.playerHand;

            // stategy works if P attacking and O does not have any trump cards
            if (gw.turn == Turn.Attacking && !oHand.Exists(c => c.suit == gw.trumpCard?.suit))
            {
                return AttackingStrategy(gw, oHand, pHand, noTrumpCards,
                    possibleCards);
            }

            if (gw.turn == Turn.Defending)
            {
                return DefendingStrategy(oHand, noTrumpCards);
            }
            return Helper.GetLowestRank(noTrumpCards);
        }

        private Card? GetCard(List<Card> possibleCards, GameView gw)
        {
            List<Card> noTrumpCards = Helper.GetCardsWithoutTrump(possibleCards, gw.trumpCard?.suit);

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
                        return Helper.GetLowestRank(possibleCards);
                    }
                    return Helper.GetLowestRank(possibleCards);
                }

                if (gw.turn == Turn.Defending)
                {
                    // Calls the defending strategy with opponent's cards based on the environment
                    return DefendingStrategy(gw.GetOpponentCards(), noTrumpCards);
                }

                return Helper.GetLowestRank(noTrumpCards);
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
                    return Helper.GetLowestRank(possibleCards);
                }
                return CallStrategy(gw, possibleCards, noTrumpCards);
            }
        }
  
        public override Card? Move(GameView gameView)
        {
            List<Card?> cards = gameView.PossibleMoves(excludePass: true);

            // cannot attack/defend
            if (cards.Count == 1 && cards[0] is null)
            {
                return null;
            }

            return GetCard(cards!, gameView); ;
        }
    }
}
