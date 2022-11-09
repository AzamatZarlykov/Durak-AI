using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

using Model.DurakWrapper;
using Model.PlayingCards;
using Model.TableDeck;
using Model.MiddleBout;
using Model.GamePlayer;
using Helpers;

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
        private bool openWorld;
        public GameStatus status => game.GetGameStatus();
        public Deck deck => game.GetDeck();
        public List<Card> discardPile => game.GetDiscardPile();
        public List<Player> players => game.GetPlayers();
        public Bout bout => game.GetBout();
        public Card? trumpCard => game.GetTrumpCard();
        public Turn turn => game.GetTurnEnum();
        public List<Card> playerHand => game.GetPlayersHand(agentIndex);
        public List<Card> opponentHand => game.GetPlayersHand((agentIndex + 1) % 2);
        public bool takes => game.GetTake();
        public bool isEarlyGame => deck.cardsLeft != 0;
        public int outcome => game.GetGameResult();
        public int plTurn => game.GetTurn();
        public bool open => openWorld;
        public bool includeTrumps => trumpCard is not null ? true : false;
        public bool isDraw => game.GetIsDraw();
        public GameView(Durak game, int agent, bool open)
        {
            this.game = game;
            this.agentIndex = agent;
            this.openWorld = open;
        }
        public GameView Copy() => new GameView(game.Copy(), agentIndex, openWorld);
        public int GetAgentIndex() => this.agentIndex;
        public List<Card?> PossibleMoves(bool excludePass) => game.PossibleMoves(excludePass);
        public override string ToString() =>
            $"\"Status\":{status}; \"Deck\":{{ {deck} }}; " +
            $"\"DiscardPile\":{Helper.toString(discardPile)}; " +
            $"\"Players\":{Helper.toString(players)}; \"Bout\":{bout}; " +
            $"\"turn\":{turn}; \"playerHand\":{Helper.toString(playerHand)}; " +
            $"\"opponentHand\":{Helper.toString(opponentHand)}; " +
            $"\"takes\":{takes}; \"isEarlyGame\":{isEarlyGame}; \"outcome\":{outcome}; \"plTurn\":{plTurn}; ";

        public void Move(Card? card) =>
            game.Move(card);

        public List<Card> GetDefendingCards(Card attacking) => 
            game.GenerateListofDefendingCards(attacking);
        public bool IsLegalDefense(Card attackingCard, Card defensiveCard) =>
            game.IsLegalDefense(attackingCard, defensiveCard);

        public List<Card> GetOpponentCards()
        {
            // when open just return the opponents hand from the Durak class
            if (openWorld)
            {
                return opponentHand;
            }

            // o/w infer opponents hand from P hand, bout and discard pile 
            // for now, just return the cards that are seen from opponent's hand

            List<Card> cards = new List<Card>();
            
            foreach (Card card in opponentHand)
            {
                if (card.GetSeen())
                {
                    cards.Add(card);
                }
            }

            Console.Write("Seen Cards: ");
            foreach(Card card in cards)
            {
                Console.Write(card + " ");
            }
            Console.WriteLine();
            return cards;
        }
    }
}