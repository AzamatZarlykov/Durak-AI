using System;
using System.Collections.Generic;
using System.Linq;

using Durak_AI.Model.PlayingCards;
using Durak_AI.Model.GamePlayer;

namespace Durak_AI.Model.TableDeck
{
    /// <summary>
    /// Deck class serves the purpose to represent the deck
    /// on the table of Durak game. 
    /// </summary>
    public class Deck
    {
        public int cardsLeft => cards.Count;
        private List<Card> cards = new List<Card>();

        // initializes the deck by creating all the ranks
        // with the corresponding suits and store in the cards list
        public Deck()
        {
            for (int suit = 0; suit < 4; suit++)
            {
                for (int rank = 6; rank < 15; rank++)
                {
                    cards.Add(new Card((Suit)suit, (Rank)rank));
                }
            }
        }


        // Shuffles the deck of cards using Fisher-Yates shuffle
        // https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
        public void Shuffle()
        {
            Random rGen = new Random();

            for (int i = cards.Count() - 1; i > 0; i--)
            {
                int indexGen = rGen.Next(i + 1);
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

        // Draws the cards from the deck and adds to players hand
        public void UpdatePlayersHand(Player player)
        {
            while (player.GetNumberOfCards() < 6 && cards.Count != 0)
            {
                player.GetHand().Add(DrawCard());
            }
        }

        // Returns the drawn card from the deck if there is any left
        public Card DrawCard()
        {
            Card card = cards.Last();
            cards.RemoveAt(cardsLeft - 1);
            return card;
        }

        // Returns the list of cards to draw to 6 cards
        public List<Card> DrawUntilSix(int numberOfCards)
        {
            List<Card> cardsToDraw = new List<Card>();

            for (int i = 0; i < 6 - numberOfCards && cardsLeft > 0; i++)
            {
                cardsToDraw.Add(DrawCard());
            }
            return cardsToDraw;
        }
    }
}
