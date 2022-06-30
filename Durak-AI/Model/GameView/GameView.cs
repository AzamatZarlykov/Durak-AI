using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.DurakWrapper;
using Model.PlayingCards;
using Model.TableDeck;

namespace Model.GameState
{
    /// <summary>
    /// Object that contains information of the current state of the game
    /// It is used to give the context for players(AI) to know what changes 
    /// were made
    /// </summary>
    public class GameView
    {
        private Durak game;

        public Deck deck => game.GetDeck();
        public Suit trumpSuit => game.GetTrumpCard().suit;
        public Turn turn => game.GetTurnEnum();

        public bool takes => game.GetTake();

        public List<Card> attackingCards => game.GetBout().GetAttackingCards();
        public List<Card> defendingCards => game.GetBout().GetDefendingCards();

        public GameView (Durak game)
        {
            this.game = game;
        }

        public List<Card> PossibleCards()
        {
            return game.PossibleCards();
        }
    }
}
