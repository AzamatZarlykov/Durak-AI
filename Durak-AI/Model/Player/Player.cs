﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.PlayingCards;
using Helpers;

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
        private string name;
        private PlayerState state;
        private List<Card> hand = new List<Card>();
        public Player(string name)
        {
            this.name = name;
        }

        public Player Copy()
        {
            Player copy = (Player)this.MemberwiseClone();

            // copy.hand = new List<Card>(hand);
            copy.hand = hand.ConvertAll(card => card.Copy());
            return copy;
        }

        public override string ToString() =>
            $"{{ \"State\":{state}; \"Hand\":{Formatter.toString(hand)} }}";
            
        public string GetName() => name;

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
    }
}
