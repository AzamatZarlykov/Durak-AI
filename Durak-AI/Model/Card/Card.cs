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
            return "Card has a rank " + rank + " and suit " + suit + "s";
        }
    }
}
