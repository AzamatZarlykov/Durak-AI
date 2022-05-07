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

        private bool defenderTakes;
        private bool isDraw;

        private int bouts;
        private int moves;

        private DiscardedPile discardedPile = new DiscardedPile();
        private List<Player> players = new List<Player>();

        private const int NUMBEROFPLAYERS = 2;
        private const int TOTALCARDS = 6;

        public Card GetTrumpCard() => trumpCard;
        public Deck GetDeck() => deck;
        public int GetDefendingPlayer() => (attackingPlayer + 1) % NUMBEROFPLAYERS;
        public int GetAttackingPlayer() => attackingPlayer;
        public Bout GetBout() => bout;
        public Turn GetTurnEnum() => turn;
        public bool GetTake() => defenderTakes;
        public bool GetIsDraw() => isDraw;

        private void FillPlayerHand(List<Card> cards, Player player)
        {
            foreach (Card card in cards)
            {
                writer.WriteVerbose(card + " ");
                player.GetHand().Add(card);
            }
            writer.WriteLineVerbose();
        }

        // Distributes, at the start of the game, the cards to players
        public void DistributeCardsToPlayers()
        {
            if (deck.cardsLeft < 12)
            {
                int a = deck.cardsLeft % 2 == 0 ? deck.cardsLeft / 2 : deck.cardsLeft / 2 + 1;
                int b = deck.cardsLeft - a;

                writer.WriteVerbose("Player0 cards: ");
                FillPlayerHand(deck.DrawCards(a), players[0]);
                writer.WriteVerbose("Player1 cards: ");
                FillPlayerHand(deck.DrawCards(b), players[1]);
                return;
            }
            writer.WriteLineVerbose();
            writer.WriteVerbose("Player0 cards: ");
            FillPlayerHand(deck.DrawCards(6), players[0]);

            writer.WriteVerbose("Player1 cards: ");
            FillPlayerHand(deck.DrawCards(6), players[1]);


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

            writer.WriteLineVerbose("Attacking player: " + attackingPlayer);
            writer.WriteLineVerbose();
        }

        // Clone of the game. VERIFY!!!
        public Durak Clone()
        {
            Durak game = new Durak(deck.GetRankStart(), deck.GetSeed(), writer);

            game.deck = deck;
            game.bout = bout;
            game.trumpCard = trumpCard;
            game.discardedPile = discardedPile;
            game.players = players;

            return game;
        }

        public Durak(int rankStartingPoint, int seed, IWriter w)
        {
            bout = new Bout();
            trumpCard = new Card();

            deck = new Deck(rankStartingPoint, seed);
            writer = w;
        }

        public void Initialize()
        {
            gameStatus = GameStatus.GameInProcess;
            isDraw = false;
            bouts = 0;
            moves = 0;

            // instantiate the deck 
            deck.Init();

            // the last card is the trump card(the one at the bottom face up)
            trumpCard = deck.GetCard(0);

            // instantiate the bout of the game
            bout = new Bout();

            // instantiate the pile
            discardedPile = new DiscardedPile();

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
                if (bout.GetAttackingCards().Exists(c => c.rank == card.rank) || 
                    bout.GetDefendingCards().Exists(c => c.rank == card.rank))
                {
                    return true;
                }

            }

            return false;
        }

        // Checks if the passed card can be used to attack in the current bout
        private bool IsAttackPossible(Card card) =>
            bout.GetAttackingCardsSize() == 0 ||
            bout.GetEverything().Exists(c => card.rank == c.rank);


        /// <summary>
        /// This method generates the list of possible attacking cards from current hand
        /// 
        /// By iterating over the player's hand, this method checks if that card's rank exists 
        /// in the game. If yes, we add to the list as it can be used to attack. O/W do not add.
        /// </summary>
        /// <param name="attCards"></param>
        /// <param name="defCards"></param>
        /// <returns></returns>
        private List<Card> GenerateListOfAttackingCards() =>
            players[attackingPlayer].GetHand().Where(IsAttackPossible).ToList();

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
        private List<Card> GenerateListofDefendingCards(Card attackingCard) =>
            players[GetDefendingPlayer()].GetHand()
                                         .Where(c => IsLegalDefense(attackingCard, c))
                                         .ToList();

        private bool CanDefend(Card attackingCard) =>
            players[GetDefendingPlayer()].GetHand()
                                         .Exists(c => IsLegalDefense(attackingCard, c));

        public List<Card> PossibleCards()
        {
            List<Card> cards = new List<Card>();

            if (turn == Turn.Attacking)
            {
                if (CanAttack())
                {
                    writer.WriteLineVerbose("Can attack", GetTurn());
                    cards = GenerateListOfAttackingCards();
                }
                else
                {
                    writer.WriteLineVerbose("cannot attack", GetTurn());
                    return cards;
                }
            } else
            {
                Card attackingCard = bout.GetAttackingCards()[^1];
                writer.WriteLineVerbose("attacking card: " + attackingCard);
                if (CanDefend(attackingCard))
                {
                    writer.WriteLineVerbose("Can defend", GetTurn());
                    cards = GenerateListofDefendingCards(attackingCard);
                }else
                {
                    writer.WriteLineVerbose("cannot defend", GetTurn());
                    return cards;
                }
            }
            
            writer.WriteVerbose("Possible cards: ", GetTurn());
            foreach (Card card in cards)
            {
                writer.WriteVerbose(card + " ", card.suit == trumpCard.suit ? 3 : GetTurn());
            }
            writer.WriteLineVerbose();

            return cards;
        }

        // returns how many players are still playing (have cards in the game)
        private bool IsDurakAssigned() =>
            players.Exists(p => p.GetState() == PlayerState.Durak);

        // return true if game is draw or durak is assigned
        private bool IsGameOver()
        {
            return IsDurakAssigned() || isDraw;
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

        private void EndGameRoleAssignment(Player first, Player second)
        {
            first.SetState(PlayerState.Winner);
            second.SetState(PlayerState.Durak);
            RemovePlayersCards(second);
        }


        private void UpdateGameStatus(Player attacker, Player defender)
        {
            if (attacker.GetNumberOfCards() == 0 && defender.GetNumberOfCards() == 0)
            {
                isDraw = true;
            }
            else if (attacker.GetNumberOfCards() == 0 && defender.GetNumberOfCards() > 0)
            {
                EndGameRoleAssignment(attacker, defender);
            }
            else if (defender.GetNumberOfCards() == 0 && attacker.GetNumberOfCards() > 0) 
            {
                EndGameRoleAssignment(defender, attacker);
            }
        }

        private bool IsEndGame(Player attacker, Player defender)
        {
            if (deck.cardsLeft != 0 && deck.GetRankStart() < 12)
            {
                return false;
            }

            UpdateGameStatus(attacker, defender);

            if (IsGameOver())
            {
                gameStatus = GameStatus.GameOver;
                return true;
            }
            return false;
        }

        private void EndBoutProcess(Player attacker, Player defender)
        {
            writer.WriteLineVerbose();
            writer.WriteVerbose("Attacker Drew: ");
            FillPlayerHand(deck.DrawCards(TOTALCARDS - attacker.GetHand().Count), attacker);
            writer.WriteVerbose("Defender Drew: ");
            FillPlayerHand(deck.DrawCards(TOTALCARDS - defender.GetHand().Count), defender);

            if (!defenderTakes)
            {
                discardedPile.AddCards(bout.GetEverything());
            } else
            {
                defenderTakes = false;
            }

            if (discardedPile.GetSize() > 0)
            {
                writer.WriteLineVerbose("Discarded Pile size: " + discardedPile.GetSize());
            }
            bout.RemoveCards();
            writer.WriteLineVerbose();

            bouts++;
        }


        public void Move(Card? card)
        {
            Player attacker = players[attackingPlayer];
            Player defender = players[GetDefendingPlayer()];

            if (turn == Turn.Attacking)
            {
                moves++;
                writer.WriteLineVerbose("Attacker's cards before: " + attacker);
                if (card is not null)
                {
                    writer.WriteVerbose("Attacks: ");
                    writer.WriteLineVerbose(card.ToString(), card.suit == trumpCard.suit ? 3 : GetTurn());
                    attacker.GetHand().Remove(card);
                    writer.WriteLineVerbose("Attacker's cards after: " + attacker);

                    bout.AddAttackingCard(card, writer);
                }
                else
                {
                    // means PASS - cannot attack
                    writer.WriteLineVerbose("PASSES", GetTurn());
                    if (!IsEndGame(attacker, defender))
                    {
                        EndBoutProcess(attacker, defender);
                        attackingPlayer = (attackingPlayer + 1) % NUMBEROFPLAYERS;
                        writer.WriteLineVerbose("changed roles");
                        writer.WriteLineVerbose();
                        return;
                    }
                }
            }
            else
            {
                moves++;
                writer.WriteLineVerbose("Defender's cards before: " + defender);
                if (card is not null)
                {
                    writer.WriteVerbose("Defends: ");
                    writer.WriteLineVerbose(card.ToString(), card.suit == trumpCard.suit ? 3 : GetTurn());
                    defender.GetHand().Remove(card);
                    writer.WriteLineVerbose("Defender's cards after: " + defender);

                    bout.AddDefendingCard(card, writer);
                }
                else
                {
                    writer.WriteLineVerbose("TAKES");
                    defenderTakes = true;
                    if (CanAttack())
                    {
                        turn = Turn.Attacking;
                        writer.WriteLineVerbose("ATTACKER ADDS EXTRA", GetTurn());
                        return;
                    }
                    if (!IsEndGame(attacker, defender))
                    {
                        FillPlayerHand(bout.GetEverything(), defender);
                        EndBoutProcess(attacker, defender);
                    }
                }
            }
            // change the agent's turn
            turn = turn == Turn.Attacking ? Turn.Defending : Turn.Attacking;
            writer.WriteLineVerbose(turn + " turn");
        }

        public int GetTurn()
        {
            return turn == Turn.Attacking ? 0 + attackingPlayer : (1 + attackingPlayer) % NUMBEROFPLAYERS;
        }

        public int GetGameResult()
        {
            if (isDraw)
            {
                return 2;
            }
            return players[0].GetState() == PlayerState.Winner ? 0 : 1;
        }

        public int GetBoutsCount() => bouts;

        public int GetMovesPerBout() => moves / bouts;
    }
}
