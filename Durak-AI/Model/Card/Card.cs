using System;
using System.Collections.Generic;

namespace Model.PlayingCards
{
    public class Card : IEquatable<Card>
    {
        public readonly Suit suit;
        public readonly Rank rank;

        private string[] suitUnicode = { "♠", "♥", "♦", "♣" };

        public Card() { }

        public Card(Suit _suit, Rank _rank)
        {
            suit = _suit;
            rank = _rank;
        }

        public string GetSuit(int index)
        {
            return suitUnicode[index];
        }

        public string GetRank(int value)
        {
            if (value < 11) { return value.ToString(); }

            return "JQKA"[value - 11].ToString();
        }

        public override string ToString()
        {
            return GetRank((int)rank) + "" + GetSuit((int)suit) + " ";
        }

        public bool Equals(Card ?other)
        {
            //Check for null and compare run-time types.
            if ((other == null) || !this.GetType().Equals(other.GetType()))
            {
                return false;
            }
            else
            {
                Card c = (Card)other;
                return (suit == (Suit)c.suit) && (rank == (Rank)c.rank);
            }
        }
    }
}
