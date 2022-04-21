using System.Collections.Generic;
using System.Linq;

using Model.MiddleBout;
using Model.PlayingCards;
using Model.TableDeck;
using Model.GamePlayer;
using Model.GameState;
using Model.DiscardedHeap;
using System;

namespace Model.DurakWrapper
{
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
        public GameStatus gameStatus;

        private Bout bout;
        private Deck deck;
        private Card trumpCard;

        private int defendingPlayer;
        private int attackingPlayer;

        private DiscardedPile discardedPile;

        private GameView gameView;

        private int numberOfTurns;

        private const int UPPER_BOUND = 1000;
        private const int NUMBEROFPLAYERS = 2;

        private List<Player> players = new List<Player>();
        
        public Card GetTrumpCard() => trumpCard;
        public Deck GetDeck() => deck;
        public int GetDefendingPlayer() => defendingPlayer;
        public int GetAttackingPlayer() => attackingPlayer;
        public Player GetPlayer(int index) => players[index];
        public Bout GetBout() => bout;

        // Distributes, at the start of the game, the cards to players
        public void DistributeCardsToPlayers()
        {
            foreach (Player p in players)
            {
                p.AddCardsToHand(deck.DrawUntilSix(0));
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
            Player pl = null;
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
            defendingPlayer = (attackingPlayer + 1) % NUMBEROFPLAYERS;
        }

        private Player GetPlayerType(string type)
        {
            if (type == "randomAI")
            {
                return new RandomAI();
            }
            return null;
        }

        public Durak(string typeA, string typeB)
        {
            gameStatus = GameStatus.GameInProcess;
            // instantiate the deck 
            deck = new Deck();
            deck.Shuffle();

            // the last card is the trump card(the one at the bottom face up)
            trumpCard = deck.GetCard(0);

            // instantiate the bout of the game
            bout = new Bout();

            // add players 
            players.Add(GetPlayerType(typeA));
            players.Add(GetPlayerType(typeB));

            // Each player draws 6 cards
            DistributeCardsToPlayers();

            // Set the attacking player
            SetAttacker();

            // run the game
            Run();
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


        private void EndBoutProcess(Player attacker, Player defender)
        {
            // update attacking and defending players hand
            deck.UpdatePlayersHand(attacker);
            deck.UpdatePlayersHand(defender);

            discardedPile.AddCards(bout.GetEverything());
            bout.RemoveCards();
        }

        private void CheckIfPlayerIsWinner(Player player)
        {
            if (!IsGameOver() && player.GetNumberOfCards() == 0)
            {
                player.SetState(PlayerState.Winner);
            }
        }

        private bool IsEndGame(Player attacker, Player defender, bool fromAttacker=true)
        {
            if (fromAttacker)
            {
                CheckIfPlayerIsWinner(attacker);
                CheckIfPlayerIsWinner(defender);
            } else
            {
                CheckIfPlayerIsWinner(attacker);
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


        // main game logic method
        public void Run()
        {
            Player attacker, defender;
            numberOfTurns = 1;
            while (gameStatus != GameStatus.GameOver && numberOfTurns < UPPER_BOUND)
            {
                attacker = GetPlayer(attackingPlayer);
                defender = GetPlayer(defendingPlayer);
                while (attacker.CanAttack(new GameView(this)))
                {
                    Card attackingCard = attacker.Attack(new GameView(this));
                    bout.AddAttackingCard(attackingCard);
                    if (defender.CanDefend(new GameView(this)))
                    {
                        Card defendingCard = defender.Defend(new GameView(this));
                        bout.AddDefendingCard(defendingCard);
                        if (!attacker.CanAttack(new GameView(this)))
                        {
                            if (!IsEndGame(attacker, defender))
                            {
                                attackingPlayer = (attackingPlayer + 1) % NUMBEROFPLAYERS;
                                defendingPlayer = (attackingPlayer + 1) % NUMBEROFPLAYERS;
                            }
                            break;
                        }
                    }
                    else
                    {
                        if (!IsEndGame(attacker, defender, false))
                        {
                            defender.AddCardsToHand(bout.GetEverything());
                        }
                    }
                }
                EndBoutProcess(attacker, defender);
                numberOfTurns++;
            }


        }
    }
}
