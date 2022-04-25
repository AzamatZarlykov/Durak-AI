using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.PlayingCards;

namespace Model.GamePlayer
{
    /// <summary>
    /// Enum class represents the states the player
    /// in during the standard variation Durak game
    /// </summary>
    public enum PlayerState
    {
        Playing,
        Winner,
        Durak
    }

    public class Player
    {
        private PlayerState state;

        private List<Card> hand = new List<Card>();

        public PlayerState GetState() => state;

        public List<Card> GetHand() => hand;

        public int GetNumberOfCards() => hand.Count;

        public void SetState(PlayerState s)
        {
            state = s; 
        }

        public void AddCardsToHand(List<Card> cards)
        {
            foreach (Card card in cards)
            {
                hand.Add(card);
            }
        }

        // Removes all cards from the player's hand
        public void RemoveAllCardsFromHand()
        {
            hand.Clear();
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder();
            foreach(Card card in hand)
            {
                res.Append(card);
            }
            return res.ToString();
        }
    }
}
