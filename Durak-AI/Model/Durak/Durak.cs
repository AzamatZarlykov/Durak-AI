using System.Collections.Generic;
using System.Linq;
using System;

using Helpers;
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
        public bool isOpen { get; set; }
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
        public int GetNextTurn() => turn == Turn.Attacking ? GetDefendingPlayer() : attackingPlayer;

        public Durak(int rankStartingPoint, bool verbose, bool isDebug, bool includeTrumps, bool open,
            int seed, string[] agents)
        {
            Random random = new Random(seed);

            isOpen = open;
            bout = new Bout();
            deck = new Deck(rankStartingPoint, random);
            writer = new Writer(Console.Out, verbose, isDebug);

            if (includeTrumps)
            {
                trumpCard = DetermineTrumpCard(random);
            }
            discardPile = new List<Card>();


            gameStatus = GameStatus.GameInProcess;
            isDraw = false;
            defenderTakes = false;
            bouts = 1;
            moves = 0;

            Info();

            // instantiate players 
            AddPlayers(agents);

            // Each player draws 6 cards
            DistributeCardsToPlayers();

            // Set the attacking player
            SetAttacker(random);
        }


        public Durak Copy()
        {
            // if original game then you're not allowed to make a copy without shuffling (closed).
            if (!isCopy && !isOpen)
            {
                throw new Exception("Cannot create a copy of Durak w/o " +
                    "shuffling in the closed environment");
            }

            Durak copy = (Durak)this.MemberwiseClone();

            copy.isCopy = true;
            copy.bout = this.bout.Copy();
            copy.deck = this.deck.Copy();
            copy.players = players.ConvertAll(p => p.Copy());
            copy.discardPile = discardPile.ConvertAll(card => card.Copy());

            return copy;
        }

        public Durak ShuffleCopy()
        {
            // perform copy
            Durak copy = (Durak)this.MemberwiseClone();

            copy.isCopy = true;
            copy.bout = this.bout.Copy();
            copy.deck = this.deck.Copy();
            copy.players = players.ConvertAll(p => p.Copy());
            copy.discardPile = discardPile.ConvertAll(card => card.Copy());

            Card? tCard = null;
            // remove the seen card (trump card) from the deck 
            if (copy.deck.cardsLeft > 0)
            {
                tCard = copy.deck.GetCard(0);
                copy.deck.RemoveCard(0);
            }

            Player opponent = copy.players[GetNextTurn()];
            var opponentHiddenCards = opponent.GetHand().Where(c => !c.GetSeen());
            int totalHiddenCards = opponentHiddenCards.Count();

            // add hidden cards from opponent's hand to the hidden deck cards
            copy.deck.GetCards().AddRange(opponentHiddenCards);
            // remove hidden cards from the hand
            opponent.GetHand().RemoveAll(card => !card.GetSeen());

            // shuffle the unseen cards 
            copy.deck.Shuffle();

            // redistribute back totalCards amount to opponent
            FillPlayerHand(copy.deck.DrawCards(totalHiddenCards), opponent, 
                $"Opponent drew random cards: ", false);

            if (tCard is not null)
            {
                // set the trump card back to the deck
                copy.deck.GetCards().Insert(0, tCard!);
            }
            return copy;
        }


        // Method that returns the outcome of the game
        // -1 - draw; 0 - player 1 won; 1 - player 2 won
        public int GetGameResult()
        {
            if (isDraw)
            {
                return -1;
            }
            return players[0].GetState() == PlayerState.Winner ? 0 : 1;
        }

        private void FillPlayerHand(List<Card> cards, Player player, string text, bool sort = true)
        {
            writer.WriteVerbose(text, isCopy);

            foreach (Card card in (sort ? Formatter.SortCards(cards) : cards))
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
                Card t = deck.GetCard(0);
                t.SetSeen(true);    // trump card is always visible to players
                return t;
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

            foreach (Card card in Formatter.SortCards(cards))
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

        // checks if defending player can take more cards from the attacker
        // in the current game state
        private bool OpponentCanFitMoreCards()
        {
            return players[GetDefendingPlayer()].GetHand().Count > 
                bout.GetAttackingCardsSize() - bout.GetDefendingCardsSize();
        }

        public List<Card?> PossibleMoves(bool excludePass)
        {
            if (bout.GetAttackingCardsSize() == 0 && !isCopy)
            {
                writer.WriteLineVerbose();
                writer.WriteLineVerbose("=== New Bout ===");
                writer.WriteLineVerbose();
            }

            writer.WriteLineVerbose($"TURN: Player {GetTurn() + 1} ({players[GetTurn()].GetName()})" +
                $" ({turn})", isCopy);

            List<Card?> cards = new List<Card?>();

            if (turn == Turn.Attacking)
            {
                // if opponent does not have cards then attack nothing
                if (players[GetDefendingPlayer()].GetNumberOfCards() == 0)
                {
                    cards.Add(null);
                    return cards;
                }
                
                // If a player has cards to play based on a bout and is able to add more cards
                if (CanAttack() && OpponentCanFitMoreCards())
                {
                    writer.WriteLineVerbose("Can attack", GetTurn(), isCopy);
                    cards = GenerateListOfAttackingCards()!;
                    // add null (pass option) if there is at least 1 card in the bout
                    if (!excludePass && bout.GetAttackingCardsSize() > 0)
                    {
                        DisplayCardsInOrder(cards!, "Possible cards: ", GetTurn());
                        cards.Add(null);
                        return cards;
                    }
                }
                else
                {
                    writer.WriteLineVerbose("Cannot attack", GetTurn(), isCopy);
                    cards.Add(null);
                    return cards;
                }
            } else
            {
                if (defenderTakes)
                {
                    cards.Add(null);
                    return cards;
                }

                Card attackingCard = bout.GetAttackingCards()[^1];
                if (CanDefend(attackingCard))
                {
                    writer.WriteLineVerbose("Can defend", GetTurn(), isCopy);
                    cards = GenerateListofDefendingCards(attackingCard)!;
                    if (!excludePass)
                    {
                        DisplayCardsInOrder(cards!, "Possible cards: ", GetTurn());
                        cards.Add(null);
                        return cards;
                    }
                }else
                {
                    writer.WriteLineVerbose("cannot defend", GetTurn(), isCopy);
                    cards.Add(null);
                    return cards;
                }
            }
            DisplayCardsInOrder(cards!, "Possible cards: ", GetTurn());

            return cards;
        }

        // returns true if Durak is assigned to any of the players, o/w false
        private bool IsDurakAssigned() =>
            players.Exists(p => p.GetState() == PlayerState.Durak);

        // return true if game is draw or durak is assigned
        private bool IsGameOver()
        {
            return IsDurakAssigned() || isDraw;
        }

        // Function that removes the cards from the last player
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

        private int CardsNeededForPlayerFromDeck(Player p)
        {
            int pCards = p.GetNumberOfCards();

            if (pCards >= TOTALCARDS) return 0;
            return TOTALCARDS - pCards;
        }

        private bool EnoughCardsInTheDeck(Player attacker)
        {
            int attackerCardNeeds = CardsNeededForPlayerFromDeck(attacker);

            if (attackerCardNeeds >= deck.cardsLeft)
                return false;
            return true;
        }

        private bool IsEndGame(Player attacker, Player defender)
        {
            if (EnoughCardsInTheDeck(attacker) && deck.GetRankStart() < 12)
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
            writer.WriteLineVerbose(isCopy);

            if (discardPile.Count > 0)
            {
                writer.WriteLineVerbose("Discard pile size: " + discardPile.Count, isCopy);
            }
            bout.RemoveCards();

            bouts++;
        }

        // Action is valid if the card belongs to the hand of the moving player
        // null is valid
        public bool ValidAction(Card? card, Player attacker, Player defender) =>
            card is null ||
            turn == Turn.Attacking && attacker.GetHand().Contains(card) ||
            turn == Turn.Defending && defender.GetHand().Contains(card);

        public bool Move(Card? card)
        {
            Player attacker = players[attackingPlayer];
            Player defender = players[GetDefendingPlayer()];

            // check if move is valid
            if (!ValidAction(card, attacker, defender)) 
                return false;

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
                        return true;
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
                    writer.WriteLineVerbose("TAKES\n", isCopy);
                    defenderTakes = true;
                    if (CanAttack() && OpponentCanFitMoreCards())
                    {
                        turn = Turn.Attacking;
                        writer.WriteLineVerbose("ATTACKER ADDS EXTRA", GetTurn(), isCopy);
                        return true;
                    }
                    if (!IsEndGame(attacker, defender))
                    {
                        EndBoutProcess(attacker, defender);
                    }
                }
            }
            // change the agent's turn
            turn = turn == Turn.Attacking ? Turn.Defending : Turn.Attacking;
            return true;
        }
    }
}
