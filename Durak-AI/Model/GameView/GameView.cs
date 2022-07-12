using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.DurakWrapper;
using Model.PlayingCards;
using Model.TableDeck;
using Model.MiddleBout;

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
        private int agentIndex;


        public GameView Copy() => new GameView(game.Copy(), agentIndex);
        public GameStatus status => game.GetGameStatus();
        public Deck deck => game.GetDeck();
        public List<Card> discardPile => game.GetDiscardPile();
        public Bout bout => game.GetBout();
        public Suit trumpSuit => game.GetTrumpCard().suit;
        public Turn turn => game.GetTurnEnum();
        public List<Card> playerHand => game.GetPlayersHand(agentIndex);
        public List<Card> opponentHand => game.GetPlayersHand((agentIndex + 1) % 2);
        public bool takes => game.GetTake();
        public List<Card> PossibleCards() => game.PossibleCards();
        public bool isEarlyGame => deck.cardsLeft != 0;
        public int outcome => game.GetGameResult();
        public int plTurn => game.GetTurn();
        public GameView (Durak game, int agent)
        {
            this.game = game;
            this.agentIndex = agent;
        }

        public void Move(Card? card) => game.Move(card);

        public bool IsLegalDefense(Card attackingCard, Card defensiveCard) =>
            game.IsLegalDefense(attackingCard, defensiveCard);
    }
}