using System;
using System.Collections.Generic;
using System.Linq;

using Model.PlayingCards;
using Helpers;

namespace Model.TableDeck
{
    /// <summary>
    /// Deck class serves the purpose to represent the deck
    /// on the table of Durak game. 
    /// </summary>
    public class Deck
    {
        private readonly int rankStart;

        private Random random = new Random();
        private List<Card> cards = new List<Card>();

        public int cardsLeft => cards.Count;
        public int GetRankStart() => rankStart;    
        public List<Card> GetCards() => cards;

        public Deck(int rankStartingPoint)
        {
            this.rankStart = rankStartingPoint;
        }

        public Deck(int ranskStartingPoint, List<Card> deckCards)
        {
            cards = new List<Card>(deckCards);
            rankStart = ranskStartingPoint;
        }

        public override string ToString() =>
            $"\"cards_left\":{cardsLeft}, \"Cards\":{Helper.toString(cards)}";

        public Deck Copy()
        {
            Deck copy = (Deck)this.MemberwiseClone();
            // copy cards from the original deck to the copy
            copy.cards = new List<Card>(cards);
            return copy;
        }

        public void Init(Random random)
        {
            for (int suit = 0; suit < 4; suit++)
            {
                for (int rank = rankStart; rank < 15; rank++)
                {
                    cards.Add(new Card((Suit)suit, (Rank)rank));
                }
            }
            this.random = random;
            Shuffle();
        }


        // Shuffles the deck of cards using Fisher-Yates shuffle
        // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        public void Shuffle()
        {
            for (int i = cards.Count() - 1; i > 0; i--)
            {
                int indexGen = random.Next(i + 1);
                (cards[i], cards[indexGen]) = (cards[indexGen], cards[i]);
            }
        }

        

        // Returns the card from the deck given the index
        public Card GetCard(int index)
        {
            try
            {
                return cards[index];
            }
            catch (ArgumentOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("index", index, "Card should be between 0 and 35"));
            }
        }

        // Returns the drawn card from the deck if there is any left
        public Card DrawCard()
        {
            Card card = cards.Last();
            RemoveCard(cardsLeft - 1);
            return card;
        }

        public void RemoveCard(int index)
        {
            cards.RemoveAt(index);
        }

        // Returns the list of cards to draw to 6 cards
        public List<Card> DrawCards(int numberOfCards)
        {
            List<Card> cardsToDraw = new List<Card>();

            for (int i = 0; i < numberOfCards && cardsLeft > 0; i++)
            {
                cardsToDraw.Add(DrawCard());
            }
            return cardsToDraw;
        }

    }
}
