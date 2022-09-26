using System.Collections.Generic;
using System.Linq;
using System;

using Helpers.Writer;
using Model.MiddleBout;
using Model.PlayingCards;
using Model.TableDeck;
using Model.GamePlayer;
using Model.GameState;

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
        private Card? trumpCard;
        // private DiscardPile discardPile;

        private int attackingPlayer;
        private Turn turn;

        private bool defenderTakes;
        private bool isDraw;

        private int bouts;
        private int moves;

        private List<Card> discardPile = new List<Card>();
        private List<Player> players = new List<Player>();

        private const int NUMBEROFPLAYERS = 2;
        private const int TOTALCARDS = 6;

        public bool isCopy { get; set; }
        public Card? GetTrumpCard() => trumpCard;
        public Deck GetDeck() => deck;
        public Bout GetBout() => bout;
        public List<Player> GetPlayers() => players;
        public List<Card> GetDiscardPile() => discardPile;
        public int GetDefendingPlayer() => (attackingPlayer + 1) % NUMBEROFPLAYERS;
        public int GetAttackingPlayer() => attackingPlayer;
        public Turn GetTurnEnum() => turn;
        public GameStatus GetGameStatus() => gameStatus;
        public bool GetTake() => defenderTakes;
        public bool GetIsDraw() => isDraw;
        public int GetBoutsCount() => bouts;
        public double GetMovesPerBout() => (double)moves / bouts;
        public List<Card> GetPlayersHand(int playerIndex) => players[playerIndex].GetHand();
        public int GetTurn() => turn == Turn.Attacking ? attackingPlayer : GetDefendingPlayer();
        public bool WithTrumpCards() => trumpCard is null ? false : true;

        // Constructor for inner game simulation for minimax agent
        public Durak(GameView gw)
        {
            // Set all the values of the respective fields
            gameStatus = gw.status;
            trumpCard = gw.trumpCard;
            attackingPlayer = gw.plTurn;
            turn = gw.turn;
            defenderTakes = gw.takes;
            isDraw = gw.isDraw;


            // copy the bout of the game state
            bout = new Bout(gw.bout.GetAttackingCards(), gw.bout.GetDefendingCards());
            // copy the deck of the game state
            deck = new Deck(gw.deck.GetRankStart(), gw.deck.GetCards());
            // initialize debugger mode to false
            writer = new Writer(Console.Out, false, false);
        }
        public Durak(int rankStartingPoint, bool verbose, bool isDebug, bool noTrumps)
        {
            if (!noTrumps)
            {
                trumpCard = new Card();
            }
            bout = new Bout();
            deck = new Deck(rankStartingPoint);
            writer = new Writer(Console.Out, verbose, isDebug);
        }

        public Durak Copy(bool open)
        {
            if (!open)
            {
                throw new Exception("The state of game is hidden");
            }
            Durak copy = (Durak)this.MemberwiseClone();

            copy.isCopy = true;
            copy.bout = this.bout.Copy();
            copy.deck = this.deck.Copy();
            copy.players = players.ConvertAll(p => p.Copy());

            copy.discardPile = new List<Card>(discardPile);

            return copy;
        }

        public int GetGameResult()
        {
            if (isDraw)
            {
                return 0;
            }
            return players[0].GetState() == PlayerState.Winner ? 1000 : -1000;
        }

        private void FillPlayerHand(List<Card> cards, Player player, string text)
        {
            writer.WriteVerbose(text, isCopy);

            cards = cards.OrderBy(c => (int)c.suit).ThenBy(c => (int)c.rank).ToList();

            foreach (Card card in cards)
            {
                if (trumpCard is null)
                {
                    writer.WriteVerbose(card + " ", 3, isCopy, true);
                } else
                {
                    writer.WriteVerbose(card + " ", card.suit == trumpCard.suit ? 2 : 3, isCopy, true);
                }
                player.GetHand().Add(card);
            }
            writer.WriteLineVerbose(isCopy);
        }


        // Distributes, at the start of the game, the cards to players
        public void DistributeCardsToPlayers()
        {
            if (deck.cardsLeft < 12)
            {
                int a = deck.cardsLeft % 2 == 0 ? deck.cardsLeft / 2 : deck.cardsLeft / 2 + 1;
                int b = deck.cardsLeft - a;

                FillPlayerHand(deck.DrawCards(a), players[0], $"Player 1 ({players[0].GetName()})" +
                    $" cards: ");
                FillPlayerHand(deck.DrawCards(b), players[1], $"Player 2 ({players[1].GetName()})" +
                    $" cards: ");
                return;
            }
            FillPlayerHand(deck.DrawCards(6), players[0], $"Player 1 ({players[0].GetName()}) " +
                $"cards: ");
            FillPlayerHand(deck.DrawCards(6), players[1], $"Player 2 ({players[1].GetName()}) " +
                $"cards: ");
        }

        // Function will find the player who has the card with
        // lowest rank of the trump card's suit.
        public void SetAttacker(Random random)
        {
            Player? pl = null;
            Rank lowTrump = 0;

            if (trumpCard is not null)
            {
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

            if (trumpCard is not null)
            {
                writer.WriteLineVerbose("Trump card: " + trumpCard, 2);
            }

            writer.WriteLineVerbose("Deck's size: " + deck.cardsLeft);
            writer.WriteLineVerbose();
        }

        private Card DetermineTrumpCard(Random random)
        {
            if (deck.cardsLeft - NUMBEROFPLAYERS * 6 > 0)
            {
                return deck.GetCard(0);
            }

            return new Card((Suit)random.Next(4));
        }

        private void AddPlayers(string[] agents)
        {
            players.Clear();
            for (int i = 0; i < NUMBEROFPLAYERS; i++)
            {
                players.Add(new Player(agents[i]));
            }
        }

        public void Initialize(int seed, string[] agents)
        {
            Random random = new Random(seed);

            gameStatus = GameStatus.GameInProcess;
            isDraw = false;
            defenderTakes = false;
            bouts = 1;
            moves = 0;

            // instantiate the deck 
            deck.Init(random);

            // instantiate the bout of the game
            bout = new Bout();

            // instantiate the pile
            discardPile = new List<Card>();

            if (trumpCard is not null)
            {
                trumpCard = DetermineTrumpCard(random);
            }

            Info();

            // instantiate players 
            AddPlayers(agents);

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
            if (trumpCard is null)
            {
                throw new Exception("Trump cards parameter is disabled");
            }
            return card.suit == trumpCard.suit;
        }

        public bool IsLegalDefense(Card attackingCard, Card defendingCard)
        {
            if (trumpCard is not null)
            {
                return (defendingCard.suit == attackingCard.suit &&
                    defendingCard.rank > attackingCard.rank) ||
                    (IsTrumpSuit(defendingCard) && (!IsTrumpSuit(attackingCard) ||
                    (IsTrumpSuit(attackingCard) && defendingCard.rank >
                    attackingCard.rank)));
            }
            return defendingCard.suit == attackingCard.suit &&
                defendingCard.rank > attackingCard.rank;
        }


        /// <summary>
        /// This method generates the list of cards that can defend the attacking card
        /// 
        /// It iterates over the hand and checks if the card can legally defend from attacking card
        /// </summary>
        /// <param name="attackingCard"></param>
        /// <param name="trump"></param>
        /// <returns></returns>
        public List<Card> GenerateListofDefendingCards(Card attackingCard) =>
            players[GetDefendingPlayer()].GetHand()
                                         .Where(c => IsLegalDefense(attackingCard, c))
                                         .ToList();

        private bool CanDefend(Card attackingCard) =>
            players[GetDefendingPlayer()].GetHand()
                                         .Exists(c => IsLegalDefense(attackingCard, c));

        private void DisplayCardsInOrder(List<Card> cards, string text, int turn)
        {
            writer.WriteVerbose(text, isCopy);

            var sortedCards = cards.
                OrderBy(card => (int)(card.suit)).
                ThenBy(card => (int)(card.rank));

            foreach (Card card in sortedCards)
            {
                if (trumpCard is not null)
                {
                    writer.WriteVerbose(card + " ", card.suit == trumpCard.suit ? 2 : turn, isCopy, true);
                } else
                {
                    writer.WriteVerbose(card + " ", turn, isCopy, true);
                }
            }

            writer.WriteLineVerbose(isCopy);
        }

        public List<Card> PossibleCards()
        {
            if (bout.GetAttackingCardsSize() == 0 && !isCopy)
            {
                writer.WriteLineVerbose();
                writer.WriteLineVerbose("=== New Bout ===");
                writer.WriteLineVerbose();
            }

            writer.WriteLineVerbose($"TURN: Player {GetTurn() + 1} ({players[GetTurn()].GetName()})" +
                $" ({turn})", isCopy);

            List<Card> cards = new List<Card>();

            if (turn == Turn.Attacking)
            {
                if (players[GetDefendingPlayer()].GetNumberOfCards() == 0)
                {
                    return cards;
                }

                if (defenderTakes || CanAttack())
                {
                    writer.WriteLineVerbose("Can attack", GetTurn(), isCopy);
                    cards = GenerateListOfAttackingCards();
                }
                else
                {
                    writer.WriteLineVerbose("cannot attack", GetTurn(), isCopy);
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
                    writer.WriteLineVerbose("Can defend", GetTurn(), isCopy);
                    cards = GenerateListofDefendingCards(attackingCard);
                }else
                {
                    writer.WriteLineVerbose("cannot defend", GetTurn(), isCopy);
                    return cards;
                }
            }
            DisplayCardsInOrder(cards, "Possible cards: ", GetTurn());

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
                discardPile.AddRange(player.GetHand());
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
                writer.WriteLineVerbose(isCopy);
                writer.WriteLineVerbose("==== GAME OVER ====", isCopy);
                writer.WriteLineVerbose(isCopy);
                bouts++;
                return true;
            }
            return false;
        }

        private void EndBoutProcess(Player attacker, Player defender)
        {
            if (!defenderTakes)
            {
                attackingPlayer = (attackingPlayer + 1) % NUMBEROFPLAYERS;
                discardPile.AddRange(bout.GetEverything());
                writer.WriteLineVerbose("changed roles", isCopy);
            }
            else
            {
                FillPlayerHand(bout.GetEverything(), defender, "Taken Cards: ");
                defenderTakes = false;
            }

            writer.WriteLineVerbose(isCopy);
            FillPlayerHand(deck.DrawCards(TOTALCARDS - attacker.GetHand().Count),
                attacker, "Attacker Drew: ");
            FillPlayerHand(deck.DrawCards(TOTALCARDS - defender.GetHand().Count), 
                defender, "Defender Drew: ");

            DisplayCardsInOrder(players[0].GetHand(), $"Player 1 ({players[0].GetName()}) " +
                $"Cards: ", 0);
            DisplayCardsInOrder(players[1].GetHand(), $"Player 2 ({players[1].GetName()})" +
                $" Cards: ", 1);

            if (discardPile.Count > 0)
            {
                writer.WriteLineVerbose("Discard pile size: " + discardPile.Count, isCopy);
            }
            bout.RemoveCards();

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
                    writer.WriteVerbose("Attacks: ", GetTurn(), isCopy);
                    if (trumpCard is not null)
                    {
                        writer.WriteLineVerbose(card.ToString(), 
                            card.suit == trumpCard.suit ? 2 : GetTurn(), isCopy, true);
                    } else
                    {
                        writer.WriteLineVerbose(card.ToString(), GetTurn(), isCopy, true);
                    }
                    attacker.GetHand().Remove(card);

                    bout.AddCard(card, trumpCard, writer, true, bouts, isCopy);
                }
                else
                {
                    // means PASS - cannot attack
                    writer.WriteLineVerbose("PASSES", GetTurn(), isCopy);
                    if (!IsEndGame(attacker, defender))
                    {
                        EndBoutProcess(attacker, defender);
                        writer.WriteLineVerbose(isCopy);
                        return;
                    }
                }
            }
            else
            {
                if (card is not null)
                {
                    moves++; // increment moves only when card is played

                    writer.WriteVerbose("Defends: ", GetTurn(), isCopy);
                    if (trumpCard is not null)
                    {
                        writer.WriteLineVerbose(card.ToString(), 
                            card.suit == trumpCard.suit ? 2 : GetTurn(), isCopy, true);
                    } else
                    {
                        writer.WriteLineVerbose(card.ToString(),
                            GetTurn(), isCopy, true);
                    }

                    defender.GetHand().Remove(card);

                    bout.AddCard(card, trumpCard, writer, false, bouts, isCopy);

                    IsEndGame(attacker, defender);
                }
                else
                {
                    writer.WriteLineVerbose("TAKES", isCopy);
                    defenderTakes = true;
                    if (CanAttack())
                    {
                        turn = Turn.Attacking;
                        writer.WriteLineVerbose("ATTACKER ADDS EXTRA", GetTurn(), isCopy);
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
    }
}
