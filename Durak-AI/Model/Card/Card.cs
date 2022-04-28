using Helpers.Unicode;

namespace Model.PlayingCards
{
    public class Card
    {
        public readonly Suit suit;
        public readonly Rank rank;

        public Card() { }
        public Card(Suit _suit, Rank _rank)
        {
            suit = _suit;
            rank = _rank;
        }

        public override string ToString()
        {
            return UnicodeConverter.GetRank((int)rank) + "" + UnicodeConverter.GetSuit((int)suit) + " ";
        }
    }
}
