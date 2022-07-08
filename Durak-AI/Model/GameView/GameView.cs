using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.DurakWrapper;
using Model.PlayingCards;
using Model.TableDeck;
using Model.DiscardHeap;
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

        public Deck deck => game.GetDeck();
        public DiscardPile discardPile => game.GetDiscardPile();
        public Bout bout => game.GetBout();
        public Suit trumpSuit => game.GetTrumpCard().suit;
        public Turn turn => game.GetTurnEnum();
        public List<Card> playerHand => game.GetPlayersHand(agentIndex);
        public bool takes => game.GetTake();
        public List<Card> PossibleCards() => game.PossibleCards();
        public bool isEarlyGame => deck.cardsLeft != 0;
        public GameView (Durak game, int agent)
        {
            this.game = game;
            this.agentIndex = agent;
        }

        private bool IsWeakness(Card card, List<Card> opponentHand) =>
            opponentHand.Any(c => c.suit == card.suit && c.rank > card.rank);

        public List<Card> GetWeaknesses(List<Card> hand, List<Card> opponentHand)
        {
            List<Card> cards = new List<Card>();
                
            foreach (Card card in hand)
            {
                if (IsWeakness(card, opponentHand))
                {
                    cards.Add(card);
                }
            }

            return cards;
        }

    }
}