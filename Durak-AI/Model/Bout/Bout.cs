using System;
using System.Collections.Generic;
using System.Linq;

using Model.PlayingCards;
using Helpers;

namespace Model.MiddleBout
{
    /// <summary>
    /// Bout class is the gameplay at the center of the board
    /// This Class represents the cards being currently played
    /// </summary>
    public class Bout
    {
        private List<Card> attackingCards = new List<Card>();
        private List<Card> defendingCards = new List<Card>();
        public Bout() { }
        public Bout(List<Card> attacking, List<Card> defending)
        {
            attackingCards = new List<Card>(attacking);
            defendingCards = new List<Card>(defending);

        }

        public override string ToString() => 
            $"{Formatter.toString(attackingCards)}_" +
            $"{Formatter.toString(defendingCards)}";

        public int GetAttackingCardsSize() => attackingCards.Count();
        public int GetDefendingCardsSize() => defendingCards.Count();
        public List<Card> GetAttackingCards() => attackingCards;
        public List<Card> GetDefendingCards() => defendingCards;
        public Card GetAttackingCard(int index) => attackingCards[index];
        public Bout Copy()
        {
            Bout copy = (Bout)this.MemberwiseClone();
            // copy attacking cards from the original bout
            copy.attackingCards = new List<Card>(attackingCards);

            // copy defending cards from the original bout
            copy.defendingCards = new List<Card>(defendingCards);

            return copy;
        }

        public List<Card> GetEverything()
        {
            return new List<Card>(attackingCards.Concat(defendingCards));
        }

        public bool ContainsRank(Rank rank) =>
            attackingCards.Exists(card => card.rank == rank) ||
            defendingCards.Exists(card => card.rank == rank);


        public bool CheckExistingSuits(Suit suit) =>
            attackingCards.Exists(card => card.suit == suit);

        public void AddCard(Card card, Card? trump, Writer writer, bool attacking, int count, bool isCopy = false)
        {
            card.SetSeen(true);

            if (attacking)
            {
                attackingCards.Add(card);
            } else
            {
                defendingCards.Add(card);
            }
            Info(trump, writer, count, isCopy);
        }

        public void RemoveCards()
        {
            attackingCards.Clear();
            defendingCards.Clear();
        }

        private void DisplayCard(Writer writer, List<Card> cards, Card? trump, bool isCopy)
        {
            foreach (Card card in cards)
            {
                if (trump is not null)
                {
                    writer.WriteVerbose(card + " ", card.suit == trump.suit ? 2 : 3, isCopy, true);
                }
                else
                {
                    writer.WriteVerbose(card + " ", 3, isCopy, true);
                }
            }
        }

        private void Info(Card? trump, Writer writer, int count, bool isCopy = false)
        {
            writer.WriteLineVerbose(isCopy);
            writer.WriteLineVerbose($"Bout {count}:", isCopy);
            writer.WriteVerbose("Attacking cards: ", isCopy);

            DisplayCard(writer, attackingCards, trump, isCopy);

            writer.WriteLineVerbose(isCopy);

            writer.WriteVerbose("Defending cards: ", isCopy);

            DisplayCard(writer, defendingCards, trump, isCopy);

            writer.WriteLineVerbose(isCopy);
            writer.WriteLineVerbose(isCopy);
        }
    }
}
