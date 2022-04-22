using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.PlayingCards;

namespace Model.GamePlayer
{
    class Player
    {
        private List<Card> hand = new List<Card>();

        public List<Card> GetHand() => hand;

        public void AddCardsToHand(List<Card> cards)
        {
            foreach (Card card in cards)
            {
                hand.Add(card);
            }
        }

    }
}
