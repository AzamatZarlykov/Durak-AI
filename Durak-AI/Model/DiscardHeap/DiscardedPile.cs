using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.PlayingCards;

namespace Model.DiscardHeap
{
    public class DiscardPile
    {
        private List<Card> discardedPile = new List<Card>();

        public DiscardPile()
        {

        }

        public int GetSize() => discardedPile.Count; 

        public void AddCards(List<Card> cards)
        {
            foreach (Card card in cards)
            {
                discardedPile.Add(card);
            }
        }

    }
}
