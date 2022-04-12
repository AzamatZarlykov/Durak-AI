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
        private Durak game;
        public GameStatus gameStatus;
        public Card trumpCard;

        public int currentPlayerID;
        public int opponentID;

        PlayerView current = new PlayerView();
        PlayerView opponent = new PlayerView();
        
        public List<Card> hand = new List<Card>();
        public List<Card> attackingCards;
        public List<Card> defendingCards;

        private int prevDiscardedHeapValue;
        public bool discardHeapChanged;

        public int discardHeapSize => game.GetDiscardedHeapSize();
        public int deckSize => game.GetDeck().cardsLeft;

        public GameView (Durak game, int id)
        {
            this.game = game;
            this.currentPlayerID = id;
            this.opponentID = (id + 1) % 2;

            if (prevDiscardedHeapValue != discardHeapSize)
            {
                discardHeapChanged = true;
            }
            prevDiscardedHeapValue = discardHeapSize;

            AssignPlayersView();

            trumpCard = deckSize == 0 ? new Card(game.GetTrumpCard().suit, (Rank)5)
                                       : game.GetTrumpCard();

            attackingCards = game.GetBout().GetAttackingCards();
            defendingCards = game.GetBout().GetDefendingCards();

        }

        private void AssignPlayersView()
        {
            Player a = game.GetPlayer(currentPlayerID);
            Player b = game.GetPlayer(opponentID);

            current.numebrOfCards = a.GetNumberOfCards();
            current.state = a.GetState();
            
            opponent.numebrOfCards = b.GetNumberOfCards();
            opponent.state = b.GetState();
        }
    }
}
