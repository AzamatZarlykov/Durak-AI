using System;
using System.Collections.Generic;

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

    /// <summary>
    /// Player class represents the player in the game. 
    /// This class contains the functions and properties that 
    /// the player would have in Durak game
    /// </summary>
    public abstract class Player
    {
        private string name;

        private bool isTaking;
        private bool isAttackersTurn;

        private PlayerState state;

        private List<Card> playersHand = new List<Card>();
        public List<Card> GetHand() => playersHand;
        public bool IsTaking() => isTaking;
        public bool IsAttackersTurn() => isAttackersTurn;
        public int GetNumberOfCards() => playersHand.Count;
        public Card GetCard(int index) => playersHand[index];
        public string GetName() => name;
        public PlayerState GetState() => state;
        public void SetState(PlayerState state) => this.state = state;




        // abstract List<Card> attack(List<Card> bout, )




        public void SetName(string n)
        {
            name = n;
        }

        public void SetIsTaking(bool value)
        {
            isTaking = value;
        }

        public void SetIsAttackersTurn(bool value)
        {
            isAttackersTurn = value;
        }
        // adds multiple cards to the players hand
        public void AddCardsToHand(List<Card> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                playersHand.Add(cards[i]);
            }
        }
        
        // Remove the certain card from the player's hand
        public void RemoveCardFromHand(Card card)
        {
            playersHand.Remove(card);
        }

        // Removes all cards from the player's hand
        public void RemoveAllCardsFromHand()
        {
            playersHand.Clear();
        }

        // resets the properties and states of the player
        public void Reset()
        {
            isTaking = false;
            isAttackersTurn = false;
            state = PlayerState.Playing;
        }

        // prints the cards of the player
        public void PrintCards()
        {
            foreach (Card c in playersHand)
            {
                Console.WriteLine("The rank : " + c.rank + ". The suit : " + c.suit);
            }
        }
        
        // gets info of the player
        public void PrintInfo(bool isAttacker)
        {
            Console.WriteLine("PRINTING PLAYER INFORMATION:");
            Console.WriteLine("NAME: " + name);
            Console.WriteLine("IS " + (isAttacker ? "ATTACKING: " : "TAKING: ") +
                (isAttacker ? isAttackersTurn : isTaking));
            Console.WriteLine("PLAYER STATE: " + state);
            Console.WriteLine("CARDS: ");
            PrintCards();
            Console.WriteLine();
        }
    }
}
