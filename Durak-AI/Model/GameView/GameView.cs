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

        public bool IsLegalDefense(Card attackingCard, Card defensiveCard) =>
            game.IsLegalDefense(attackingCard, defensiveCard);
        
        private bool CardsInNonweakness(List<Card> defense, List<Card> nonweakness)
        {
            bool result = true;


            foreach (Card card in defense)
            {
                if (!nonweakness.Any(c => c.rank == card.rank))
                {
                    return false;
                }
            }

            return result;
        }

        public Card? GetBadlyCoveredWeakness(List<Card> pHand, List<Card> oHand, List<Card> ws)
        {
            Card? weakCard = null;
            List<Card> nonweakness = pHand.Where(card => !ws.Contains(card)).ToList();

/*            Console.Write("Nonweakness Cards: ");
            foreach (Card qw in nonweakness)
            {
                Console.Write(qw + " ");
            }
            Console.WriteLine();*/

            foreach (Card card in ws)
            {
                // Console.WriteLine("Weakness Card: " + card);

                List<Card> defensiveCards = oHand.Where(c => IsLegalDefense(card, c)).ToList();
/*
                Console.Write("Defensive Cards: ");
                foreach (Card qw in defensiveCards)
                {
                    Console.Write(qw + " ");
                }
                Console.WriteLine();*/

                if (CardsInNonweakness(defensiveCards, nonweakness))
                {
                    //Console.WriteLine("Found: " + card);
                    return card;
                    
                }
            }
            //Console.WriteLine("Didnot Find");
            return weakCard;
        }
    }
}