using System;
using System.Collections.Generic;
using System.Linq;

using Model.PlayingCards;
using Model.DiscardedHeap;

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
        
        public List<Card> GetEverything()
        {
            return new List<Card>(attackingCards.Concat(defendingCards));
        }

        public bool ContainsRank(Rank rank) =>
            attackingCards.Exists(card => card.rank == rank) ||
            defendingCards.Exists(card => card.rank == rank);


        public bool CheckExistingSuits(Suit suit) =>
            attackingCards.Exists(card => card.suit == suit);

        public void AddAttackingCard(Card card)
        {
            attackingCards.Add(card);
            Info();
        }

        public void AddDefendingCard(Card card)
        {
            defendingCards.Add(card);
            Info();
        }
        
        public void RemoveCards()
        {
            attackingCards.Clear();
            defendingCards.Clear();
        }

        private void Info()
        {
            Console.WriteLine("Bout");
            Console.Write("Attacking cards: ");
            foreach(Card card in attackingCards)
            {
                Console.Write(card);
            }
            Console.WriteLine();

            Console.Write("Defending cards: ");
            foreach (Card card in defendingCards)
            {
                Console.Write(card);
            }
            Console.WriteLine();
        }
    }
}
