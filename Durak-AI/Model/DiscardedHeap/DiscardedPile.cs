using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.PlayingCards;

namespace Model.DiscardedHeap
{
    public class DiscardedPile
    {
        private List<Card> discardedPile = new List<Card>();

        public DiscardedPile()
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
