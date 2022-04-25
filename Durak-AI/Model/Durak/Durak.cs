using System.Collections.Generic;
using System.Linq;
using System;

using Helpers.Writer;
using Model.MiddleBout;
using Model.PlayingCards;
using Model.TableDeck;
using Model.GameState;
using Model.DiscardedHeap;
using Model.GamePlayer;

namespace Model.DurakWrapper
{
    public enum Turn
    {
        Attacking, Defending
    }

    /// <summary>
    /// This enum represents the status of the game
    /// </summary>
    public enum GameStatus
    {
        GameInProcess,
        GameOver
    }

    /// <summary>
    /// Class that holds all the properties and function of Durak
    /// where only 2 players play a standard variation.
    /// </summary>
    public class Durak
    {
        private readonly IWriter writer;

        public GameStatus gameStatus;

        private Bout bout;
        private Deck deck;
        private Card trumpCard;

        private int attackingPlayer;
        private Turn turn;

        private DiscardedPile discardedPile = new DiscardedPile();
        private List<Player> players = new List<Player>();

        private const int NUMBEROFPLAYERS = 2;

        public Card GetTrumpCard() => trumpCard;
        public Deck GetDeck() => deck;
        public int GetDefendingPlayer() => (attackingPlayer + 1) % NUMBEROFPLAYERS;
        public int GetAttackingPlayer() => attackingPlayer;
        public Bout GetBout() => bout;
        public int GetTurn()
        {
            return turn == Turn.Attacking ? 0 : 1;
        }

        // Distributes, at the start of the game, the cards to players
        public void DistributeCardsToPlayers()
        {
            if (deck.cardsLeft < 12)
            {
                int a = deck.cardsLeft % 2 == 0 ? deck.cardsLeft / 2 : deck.cardsLeft / 2 + 1;
                int b = deck.cardsLeft - a;

                FillPlayerHand(deck.DrawCards(a), players[0]);
                FillPlayerHand(deck.DrawCards(b), players[1]);
                return;
            } 

            foreach (Player p in players)
            {
                FillPlayerHand(deck.DrawCards(6), p);
            }
        }

        private int GetRandomPlayerIndex()
        {
            Random rnd = new Random();
            return rnd.Next(0, NUMBEROFPLAYERS);
        }

        // Function will find the player who has the card with
        // lowest rank of the trump card's suit.
        public void SetAttacker()
        {
            Player? pl = null;
            Rank lowTrump = 0;

            foreach (Player player in players)
            {
                foreach (Card c in player.GetHand())
                {
                    if (c.suit == trumpCard.suit && (pl == null || c.rank < lowTrump))
                    {
                        pl = player;
                        lowTrump = c.rank;
                    }
                }
            }

            // If no player has a trump card then random player be attacking
            if (pl == null)
            {
                pl = players[GetRandomPlayerIndex()];
            }

            // assigning players' indices
            attackingPlayer = players.IndexOf(pl);
            turn = Turn.Attacking;
        }

        public void Info()
        {
            writer.WriteLineVerbose("Deck's size: " + deck.cardsLeft);
            writer.WriteVerbose("Trump card: " + trumpCard);
            writer.WriteLineVerbose();

            writer.WriteVerbose("Player0 cards: " + players[0]);
            writer.WriteLineVerbose();
            writer.WriteVerbose("Player1 cards: " + players[1]);
            writer.WriteLineVerbose();

            writer.WriteLineVerbose("Attacking player: " + attackingPlayer);
            writer.WriteLineVerbose();
        }

        public Durak(int rankStartingPoint, IWriter w)
        {
            deck = new Deck(rankStartingPoint);
            writer = w;
        }

        public void Initialize()
        {
            gameStatus = GameStatus.GameInProcess;

            // instantiate the deck 
            deck.Init();

            // the last card is the trump card(the one at the bottom face up)
            trumpCard = deck.GetCard(0);

            // instantiate the bout of the game
            bout = new Bout();

            players.Clear();
            players.Add(new Player());
            players.Add(new Player());
            // Each player draws 6 cards
            DistributeCardsToPlayers();

            // Set the attacking player
            SetAttacker();
            Info();
        }

        private bool CanAttack()
        {
            if (bout.GetAttackingCardsSize() == 0)
            {
                return true;
            }

            foreach (Card card in players[attackingPlayer].GetHand())
            {
                return bout.GetAttackingCards().Exists(c => c.rank == card.rank) ||
                       bout.GetDefendingCards().Exists(c => c.rank == card.rank);
            }

            return false;
        }

        // Checks if the passed card can be used to attack in the current bout
        private bool IsAttackPossible(Card card)
        {
            if (bout.GetAttackingCardsSize() == 0)
            {
                return true;
            }

            foreach (Card c in bout.GetEverything())
            {
                if (card.rank == c.rank)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This method generates the list of possible attacking cards from current hand
        /// 
        /// By iterating over the player's hand, this method checks if that card's rank exists 
        /// in the game. If yes, we add to the list as it can be used to attack. O/W do not add.
        /// </summary>
        /// <param name="attCards"></param>
        /// <param name="defCards"></param>
        /// <returns></returns>
        private List<Card> GenerateListOfAttackingCards()
        {
            List<Card> result = new List<Card>();

            foreach (Card card in players[attackingPlayer].GetHand())
            {
                if (IsAttackPossible(card))
                {
                    result.Add(card);
                }
            }

            return result;
        }

        public bool IsTrumpSuit(Card card)
        {
            return card.suit == trumpCard.suit;
        }

        public bool IsLegalDefense(Card attackingCard, Card defendingCard)
        {
            return (defendingCard.suit == attackingCard.suit &&
                    defendingCard.rank > attackingCard.rank) ||
                    (IsTrumpSuit(defendingCard) && (!IsTrumpSuit(attackingCard) ||
                    (IsTrumpSuit(attackingCard) && defendingCard.rank >
                    attackingCard.rank)));
        }


        /// <summary>
        /// This method generates the list of cards that can defend the attacking card
        /// 
        /// It iterates over the hand and checks if the card can legally defend from attacking card
        /// </summary>
        /// <param name="attackingCard"></param>
        /// <param name="trump"></param>
        /// <returns></returns>
        private List<Card> GenerateListofDefendingCards(Card attackingCard)
        {
            List<Card> result = new List<Card>();

            foreach (Card card in players[GetDefendingPlayer()].GetHand())
            {
                if (IsLegalDefense(attackingCard, card))
                {
                    result.Add(card);
                }
            }

            return result;
        }

        private bool CanDefend(Card attackingCard)
        {
            foreach (Card defendingCard in players[GetDefendingPlayer()].GetHand())
            {
                if (IsLegalDefense(attackingCard, defendingCard))
                {
                    return true;
                }
            }
            return false;
        }

        public List<Card>? PossibleCards()
        {
            List<Card>? cards = null;

            if (turn == Turn.Attacking)
            {
                if (CanAttack())
                {
                    writer.WriteLineVerbose("Can attack");
                    cards = GenerateListOfAttackingCards();
                }
                else
                {
                    writer.WriteLineVerbose("cannot attack");
                }
            } else
            {
                Card attackingCard = bout.GetAttackingCards()[^1];
                writer.WriteLineVerbose("attacking card: " + attackingCard);
                if (CanDefend(attackingCard))
                {
                    cards = GenerateListofDefendingCards(attackingCard);
                }else
                {
                    writer.WriteLineVerbose("cannot defend");
                }
            }
            
            if (cards is null)
            {
                return null;
            }

            writer.WriteLineVerbose("Possible cards: ");
            foreach (Card card in cards)
            {
                writer.WriteVerbose(card + " ");
            }
            writer.WriteLineVerbose();

            return cards;
        }

        // returns how many players are still playing (have cards in the game)
        private int GetSizeOfPlayingPlayers()
        {
            int total = 0;
            foreach (Player player in players)
            {
                if (player.GetState() == PlayerState.Playing)
                {
                    total += 1;
                }
            }
            return total;
        }

        // The game is over when there is only one playing player left
        private bool IsGameOver()
        {
            return GetSizeOfPlayingPlayers() == 1;
        }

        private void CheckIfPlayerIsWinner(Player player)
        {
            if (!IsGameOver() && player.GetNumberOfCards() == 0)
            {
                player.SetState(PlayerState.Winner);
            }
        }

        // Function that removes the cards from the last player (defending)
        private void RemovePlayersCards(Player player)
        {
            if (player.GetNumberOfCards() > 0)
            {
                discardedPile.AddCards(player.GetHand());
                player.RemoveAllCardsFromHand();
            }
        }

        private bool IsEndGame(Player attacker, Player defender, bool fromAttacker = true)
        {
            if (fromAttacker)
            {
                CheckIfPlayerIsWinner(attacker);
                CheckIfPlayerIsWinner(defender);
            }
            else
            {
                CheckIfPlayerIsWinner(players[attackingPlayer]);
            }

            if (IsGameOver())
            {
                defender.SetState(PlayerState.Durak);
                gameStatus = GameStatus.GameOver;
                RemovePlayersCards(defender);
                return true;
            }
            return false;
        }

        private void FillPlayerHand(List<Card> cards, Player player)
        {
            foreach(Card card in cards)
            {
                writer.WriteVerbose(card + " ");
                player.GetHand().Add(card);
            }
            writer.WriteLineVerbose();
        }

        private void EndBoutProcess(Player attacker, Player defender, bool takes = false)
        {
            writer.WriteVerbose("Attacker Drew: ");
            FillPlayerHand(deck.DrawCards(attacker.GetHand().Count), attacker);
            writer.WriteVerbose("Defender Drew: ");
            FillPlayerHand(deck.DrawCards(defender.GetHand().Count), defender);


            if (!takes)
            {
                discardedPile.AddCards(bout.GetEverything());
            }

            if (discardedPile.GetSize() > 0)
            {
                writer.WriteLineVerbose("Discarded Pile size: " + discardedPile.GetSize());
            }
            bout.RemoveCards();
        }


        public void Move(Card? card)
        {
            Player attacker = players[attackingPlayer];
            Player defender = players[GetDefendingPlayer()];

            if (turn == Turn.Attacking)
            {
                writer.WriteLineVerbose("Attacker's cards before: " + attacker);
                if (card is not null)
                {
                    writer.WriteLineVerbose("Attacks: " + card);
                    attacker.GetHand().Remove(card);
                    writer.WriteLineVerbose("Attacker's cards after: " + attacker);

                    bout.AddAttackingCard(card, writer);
                }
                else
                {
                    // means PASS - cannot attack
                    writer.WriteLineVerbose("PASSES");
                    if (!IsEndGame(attacker, defender))
                    {
                        attackingPlayer = (attackingPlayer + 1) % NUMBEROFPLAYERS;
                        writer.WriteLineVerbose("changed roles");
                        EndBoutProcess(attacker, defender);
                        return;
                    }
                }
            }
            else
            {
                writer.WriteLineVerbose("Defender's cards before: " + defender);

                if (card is not null)
                {
                    writer.WriteLineVerbose("Defends: " + card);
                    defender.GetHand().Remove(card);
                    writer.WriteLineVerbose("Defender's cards after: " + defender);

                    bout.AddDefendingCard(card, writer);
                } 
                else
                {
                    writer.WriteLineVerbose("TAKES");
                    if (!IsEndGame(attacker, defender, false))
                    {
                        FillPlayerHand(bout.GetEverything(), defender);
                        EndBoutProcess(attacker, defender, true);
                    }
                }
            }
            // change the agent's turn
            turn = turn == Turn.Attacking ? Turn.Defending : Turn.Attacking;
            writer.WriteLineVerbose(turn + " turn");
        }

        public int GetWinner()
        {
            return players[0].GetState() == PlayerState.Winner ? 0 : 1;
        }
    }
}
