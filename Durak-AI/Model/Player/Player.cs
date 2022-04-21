using System;
using System.Collections.Generic;

using Model.MiddleBout;
using Model.PlayingCards;
using Model.GameState;

namespace Model.GamePlayer
{
    public enum PlayerType
    {
        RandomAI, GreedyAI, MonteCarloAI
    }
    
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
        protected string name;
        protected PlayerState state;
        protected List<Card> hand = new List<Card>();

        public abstract Card Attack(GameView gameView);
        public abstract Card Defend(GameView gameView);
        public abstract bool CanDefend(GameView gameView);
        public abstract bool CanAttack(GameView gameView);

        public List<Card> GetHand() => hand;
        public PlayerState GetState() => state;

        public int GetNumberOfCards() => hand.Count;

        public void SetState(PlayerState s)
        {
            state = s;
        }


        public bool IsTrumpSuit(Card card, Card trump)
        {
            return card.suit == trump.suit;
        }

        public bool IsLegalDefense(Card attackingCard, Card defendingCard, Card trump)
        {
            return (defendingCard.suit == attackingCard.suit &&
                    defendingCard.rank > attackingCard.rank) ||
                    (IsTrumpSuit(defendingCard, trump) && (!IsTrumpSuit(attackingCard, trump) ||
                    (IsTrumpSuit(attackingCard, trump) && defendingCard.rank >
                    attackingCard.rank)));
        }

        public void AddCardsToHand(List<Card> cards)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                hand.Add(cards[i]);
            }
        }
        
        // Remove the certain card from the player's hand
        public void RemoveCardFromHand(Card card)
        {
            hand.Remove(card);
        }

        // Removes all cards from the player's hand
        public void RemoveAllCardsFromHand()
        {
            hand.Clear();
        }

        // prints the cards of the player
        public void PrintCards()
        {
            foreach (Card c in hand)
            {
                Console.WriteLine("The rank : " + c.rank + ". The suit : " + c.suit);
            }
        }
        
        // gets info of the player
        public void PrintInfo(bool isAttacker)
        {
            Console.WriteLine("PRINTING PLAYER INFORMATION:");
            Console.WriteLine("NAME: " + name);
            Console.WriteLine("CARDS: ");
            PrintCards();
            Console.WriteLine();
        }
    }
}
