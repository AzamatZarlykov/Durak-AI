using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.DurakWrapper;
using Model.PlayingCards;
using Model.GamePlayer;

namespace Model.GameState
{
    /// <summary>
    /// Object that contains the information about the player
    /// </summary>
    public class PlayerView
    {
        public int numebrOfCards;
        public PlayerState state;
    }

    /// <summary>
    /// Object that contains information of the current state of the game
    /// It is used to give the context for players(AI) to know what changes 
    /// were made
    /// </summary>
    public class GameView
    {
        public Card trumpCard;

        public List<Card> hand = new List<Card>();
        public List<Card> attackingCards;
        public List<Card> defendingCards;

        public GameView (Durak game)
        {
            trumpCard = game.GetDeck().cardsLeft != 0 ? game.GetTrumpCard() :
                                new Card(game.GetTrumpCard().suit, (Rank)5);

            attackingCards = game.GetBout().GetAttackingCards();
            defendingCards = game.GetBout().GetDefendingCards();
        }
    }
}
