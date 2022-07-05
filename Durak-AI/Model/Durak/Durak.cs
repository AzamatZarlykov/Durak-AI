using System.Collections.Generic;
using System.Linq;
using System;

using Helpers.Writer;
using Model.MiddleBout;
using Model.PlayingCards;
using Model.TableDeck;
using Model.GameState;
using Model.DiscardHeap;
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
        private readonly Writer writer;
        
        public GameStatus gameStatus;

        private Bout bout;
        private Deck deck;
        private Card trumpCard;
        private DiscardPile discardPile;

        private int attackingPlayer;
        private Turn turn;

        private bool defenderTakes;
        private bool isDraw;

        private int bouts;
        private int moves;

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
        public int GetBoutsCount() => bouts;
        public double GetMovesPerBout() => (double)moves / bouts;

        public Durak(int rankStartingPoint, bool verbose)
        {
            trumpCard = new Card();
            bout = new Bout();
            discardPile = new DiscardPile();
            deck = new Deck(rankStartingPoint);
            writer = new Writer(Console.Out, verbose);
        }

        public Durak Copy()
        {
            Durak copy = (Durak)this.MemberwiseClone();

            copy.bout = this.bout.Copy();
            copy.deck = this.deck.Copy();
            copy.trumpCard = this.trumpCard.Copy();
            copy.discardPile = this.discardPile.Copy();

            copy.players = players.ConvertAll(p => p.Copy());

            return copy;
        }

        private void FillPlayerHand(List<Card> cards, Player player)
        {
            foreach (Card card in cards)
            {
                writer.WriteVerbose(card + " ", card.suit == trumpCard.suit ? 2 : 3);
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

                writer.WriteVerbose("Player 1 cards: ", 0);
                FillPlayerHand(deck.DrawCards(a), players[0]);
                writer.WriteVerbose("Player 2 cards: ", 1);
                FillPlayerHand(deck.DrawCards(b), players[1]);
                return;
            }
            writer.WriteVerbose("Player 1 cards: ");
            FillPlayerHand(deck.DrawCards(6), players[0]);

            writer.WriteVerbose("Player 2 cards: ");
            FillPlayerHand(deck.DrawCards(6), players[1]);


        }

        // Function will find the player who has the card with
        // lowest rank of the trump card's suit.
        public void SetAttacker(Random random)
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
                pl = players[random.Next(0, NUMBEROFPLAYERS)];
            }

            // assigning players' indices
            attackingPlayer = players.IndexOf(pl);
            turn = Turn.Attacking;
        }

        public void Info()
        {
            writer.WriteLineVerbose("==== START ====");
            writer.WriteLineVerbose();

            writer.WriteLineVerbose("Trump card: " + trumpCard);
            writer.WriteLineVerbose("Deck's size: " + deck.cardsLeft);
            writer.WriteLineVerbose();
        }

        private Card DetermineTrumpCard(Random random)
        {
            if (deck.cardsLeft > 0)
            {
                return deck.GetCard(0);
            }

            return new Card((Suit)random.Next(4), (Rank)5);
        }

        public void Initialize(int seed)
        {
            Random random = new Random(seed);

            gameStatus = GameStatus.GameInProcess;
            isDraw = false;
            defenderTakes = false;
            bouts = 0;
            moves = 0;

            // instantiate the deck 
            deck.Init(random);

            trumpCard = DetermineTrumpCard(random);

            // instantiate the bout of the game
            bout = new Bout();

            // instantiate the pile
            discardPile = new DiscardPile();

            players.Clear();
            players.Add(new Player());
            players.Add(new Player());

            Info();

            // Each player draws 6 cards
            DistributeCardsToPlayers();

            // Set the attacking player
            SetAttacker(random);
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
            if (bout.GetAttackingCardsSize() == 0)
            {
                writer.WriteLineVerbose();
                writer.WriteLineVerbose("=== New Bout ===");
                writer.WriteLineVerbose();
            }

            writer.WriteLineVerbose("TURN: Player " + (GetTurn() + 1) + " (" + turn + ")");

            List<Card> cards = new List<Card>();

            if (turn == Turn.Attacking)
            {
                if (defenderTakes || CanAttack())
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
                if (defenderTakes)
                {
                    return cards;
                }

                Card attackingCard = bout.GetAttackingCards()[^1];
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
                writer.WriteVerbose(card + " ", card.suit == trumpCard.suit ? 2 : GetTurn());
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
                discardPile.AddCards(player.GetHand());
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
                writer.WriteLineVerbose();
                writer.WriteLineVerbose("==== GAME OVER ====");
                writer.WriteLineVerbose();
                return true;
            }
            return false;
        }

        private void EndBoutProcess(Player attacker, Player defender)
        {
            if (!defenderTakes)
            {
                attackingPlayer = (attackingPlayer + 1) % NUMBEROFPLAYERS;
                discardPile.AddCards(bout.GetEverything());
            }
            else
            {
                writer.WriteVerbose("Taken Cards: ");
                FillPlayerHand(bout.GetEverything(), defender);
                defenderTakes = false;
            }

            writer.WriteLineVerbose();
            writer.WriteVerbose("Attacker Drew: ");
            FillPlayerHand(deck.DrawCards(TOTALCARDS - attacker.GetHand().Count), attacker);
            writer.WriteVerbose("Defender Drew: ");
            FillPlayerHand(deck.DrawCards(TOTALCARDS - defender.GetHand().Count), defender);

            writer.WriteVerbose("Player 1 Cards: ");
            foreach (Card card in players[0].GetHand())
            {
                writer.WriteVerbose(card + " ", card.suit == trumpCard.suit ? 2 : 3);
            }
            writer.WriteLineVerbose();
            writer.WriteVerbose("Player 2 Cards: ");
            foreach (Card card in players[1].GetHand())
            {
                writer.WriteVerbose(card + " ", card.suit == trumpCard.suit ? 2 : 3);
            }
            writer.WriteLineVerbose();

            if (discardPile.GetSize() > 0)
            {
                writer.WriteLineVerbose("Discard pile size: " + discardPile.GetSize());
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
                if (card is not null)
                {
                    writer.WriteVerbose("Attacks: ", GetTurn());
                    writer.WriteLineVerbose(card.ToString(), card.suit == trumpCard.suit ? 2 : GetTurn());
                    attacker.GetHand().Remove(card);

                    bout.AddCard(card, trumpCard, writer, true);
                }
                else
                {
                    // means PASS - cannot attack
                    writer.WriteLineVerbose("PASSES", GetTurn());
                    if (!IsEndGame(attacker, defender))
                    {
                        EndBoutProcess(attacker, defender);
                        writer.WriteLineVerbose("changed roles");
                        return;
                    }
                }
            }
            else
            {
                if (card is not null)
                {
                    moves++; // increment moves only when card is played

                    writer.WriteVerbose("Defends: ", GetTurn());
                    writer.WriteLineVerbose(card.ToString(), card.suit == trumpCard.suit ? 2 : GetTurn());
                    defender.GetHand().Remove(card);

                    bout.AddCard(card, trumpCard, writer, false); ;
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
                        EndBoutProcess(attacker, defender);
                    }
                }
            }
            // change the agent's turn
            turn = turn == Turn.Attacking ? Turn.Defending : Turn.Attacking;
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
    }
}
