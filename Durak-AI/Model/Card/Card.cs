namespace Model.PlayingCards
{
    public class Card
    {
        public readonly Suit suit;
        public readonly Rank rank;

        private string[] suitUnicode = { "\u2660", "\u2665", "\u2666", "\u2663" };

        public Card() { }
        public Card(Suit _suit, Rank _rank)
        {
            suit = _suit;
            rank = _rank;
        }
        
        private string GetSuit(int index)
        {
            return suitUnicode[index];
        }

        private string GetRank(int value)
        {
            if (value < 11) { return value.ToString(); }

            switch(value)
            {
                case 11: return "J";
                case 12: return "Q";
                case 13: return "K";
                default: return "A";
            }
        }

        public override string ToString()
        {
            return GetRank((int)rank) + "" + GetSuit((int)suit) + " ";
        }
    }
}
