using System;
using System.Collections.Generic;
using System.Linq;

using Model.PlayingCards;
using Helpers.Writer;

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

        public void AddCard(Card card, Card trump, Writer writer, bool attacking, int count)
        {
            if (attacking)
            {
                attackingCards.Add(card);
            } else
            {
                defendingCards.Add(card);
            }
            Info(trump, writer, count);
        }

        public void RemoveCards()
        {
            attackingCards.Clear();
            defendingCards.Clear();
        }

        private void Info(Card trump, Writer writer, int count)
        {
            writer.WriteLineVerbose();
            writer.WriteLineVerbose($"Bout {count}:");
            writer.WriteVerbose("Attacking cards: ");
            foreach(Card card in attackingCards)
            {
                writer.WriteVerbose(card + " ", card.suit == trump.suit ? 2 : 3);
            }
            writer.WriteLineVerbose();

            writer.WriteVerbose("Defending cards: ");
            foreach (Card card in defendingCards)
            {
                writer.WriteVerbose(card + " ", card.suit == trump.suit ? 2 : 3);
            }
            writer.WriteLineVerbose();
            writer.WriteLineVerbose();
        }
    }
}
