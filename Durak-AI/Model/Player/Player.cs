using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.PlayingCards;
using Helpers.Writer;

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
        public Player()
        {
        }

        public Player Copy()
        {
            Player copy = (Player)this.MemberwiseClone();

            copy.hand = hand.ConvertAll(x => x.Copy());

            return copy;
        }
        public PlayerState GetState() => state;

        public List<Card> GetHand() => hand;

        public int GetNumberOfCards() => hand.Count;

        public void SetState(PlayerState s)
        {
            state = s; 
        }

        // Removes all cards from the player's hand
        public void RemoveAllCardsFromHand()
        {
            hand.Clear();
        }

        public override string ToString() =>
           string.Join("", hand);
    }
}
