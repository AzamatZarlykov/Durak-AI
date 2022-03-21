using System.Collections.Generic;
using System.Linq;

using Durak_AI.Model.MiddleBout;
using Durak_AI.Model.PlayingCards;
using Durak_AI.Model.TableDeck;
using Durak_AI.Model.GamePlayer;

namespace Durak_AI.Model.DurakWrapper
{
    /// <summary>
    /// This enum represents the outcome of the action of the player
    /// </summary>
    public enum MoveResult
    {
        OK,
        OutOfTurn,
        IllegalMove,
        TookCards,
        ExtraCard,
        GameIsOver,
        UseDisplayButton
    }
    
    /// <summary>
    /// This enum represents the status of the game
    /// </summary>
    public enum GameStatus 
    { 
        NotCreated, 
        GameInProcess, 
        GameOver 
    }


    /// <summary>
    /// Class that holds all the properties and function of Durak
    /// where only 2 players play a standard variation.
    /// </summary>
    public class Durak
    {
        private Bout bout;
        private Deck deck;
        private Card trumpCard;

        private int defendingPlayer;
        private int attackingPlayer;

        private int discardedHeapSize;

        private const int NUMBEROFPLAYERS = 2;

        private List<Player> players = new List<Player>();
        
        public Card GetTrumpCard() => trumpCard;
        public Deck GetDeck() => deck;
        public int GetDiscardedHeapSize() => discardedHeapSize;
        public int GetDefendingPlayer() => defendingPlayer;
        public int GetAttackingPlayer() => attackingPlayer;
        public Player GetPlayer(int index) => players[index];

        // Distributes, at the start of the game, the cards to players
        public void DistributeCardsToPlayers()
        {
            foreach (Player p in players)
            {
                p.AddCardsToHand(deck.DrawUntilSix(0));
            }
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
                    if (c.suit == trumpCard.suit && (pl == null ||
                        c.rank < lowTrump))
                    {
                        pl = player;
                        lowTrump = c.rank;
                    }
                }
            }

            // If no player has a trump card then the first player
            // connected to the game will be the attacking player
            if (pl == null)
            {
                pl = players.First();
            }

            // e.g in the game of 3 players, if attacking player is 3
            // then defending is 1
            attackingPlayer = players.IndexOf(pl);
            defendingPlayer = (attackingPlayer + 1) % NUMBEROFPLAYERS;

            GetPlayer(attackingPlayer).SetIsAttackersTurn(true);
        }

        // Method that adds the players to the game
        // Further Changes: It should take the parameter that
        // will assign the particular AI (RandomAI, MCTS AI, RL AI,
        // RuleBased AI, even a person)
        private void AddPlayers()
        {
            players.Add(new Player());
            players.Add(new Player());
        }

        public Durak()
        {
            // instantiate the deck 
            deck = new Deck();
            deck.Shuffle();

            // the last card is the trump card(the one at the bottom face up)
            trumpCard = deck.GetCard(0);

            // add players 
            AddPlayers();
            // Each player draws 6 cards
            DistributeCardsToPlayers();

            // Set the attacking player
            SetAttacker();
            
            // instantiate the bout of the game
            bout = new Bout();
        }

        // returns how many players are still playing (have cards in the game)
        private int GetSizeOfPlayingPlayers()
        {
            int total = 0;
            foreach (Player player in players)
            {
                if (player.state == PlayerState.Playing)
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

        // checks if the attacking card is extra
        private bool CheckIfAttackingCardIsExtra()
        {
            Player defender = GetPlayer(defendingPlayer);
            int leftOver = bout.GetAttackingCardsSize() + 1 - bout.GetDefendingCardsSize();

            // Case1: defender takes the cards. If the # of attacking cards (including the last one)
            // is greater than defender's hand then it is an extra card
            // Case2: defender defends all the attacking cards and attacker attacks do a simple 
            // check : if the defender still has cards left to defend last one.
            if (defender.IsTaking())
            {
                return leftOver > defender.GetNumberOfCards();
            } 
            else
            {
                return defender.GetNumberOfCards() == 0;
            }
        }

        // Called when the attacker selects the attacking card
        public MoveResult AttackerMove(int cardIndex)
        {
            Player player = GetPlayer(attackingPlayer);
            if (IsGameOver())
            {
                return MoveResult.GameIsOver;
            }

            // if the attack started wait for the defense
            if (!player.IsAttackersTurn())
            {
                return MoveResult.OutOfTurn;
            }

            Card attackingCard = player.GetCard(cardIndex);

            if (bout.GetAttackingCardsSize() != 0 && !bout.ContainsRank(attackingCard.rank))
            {
                return MoveResult.IllegalMove;
            }

            if (CheckIfAttackingCardIsExtra())
            {
                return MoveResult.ExtraCard;
            }

            bout.AddAttackingCard(attackingCard);
            player.RemoveCardFromHand(attackingCard);

            bout.SetBoutChanged(true);

            if (!GetPlayer(defendingPlayer).IsTaking())
            {
                player.SetIsAttackersTurn(false);
            }

            return MoveResult.OK;
        }

        // returns true if card's suit match the trump's
        public bool IsTrumpSuit(Card card)
        {
            return card.suit == trumpCard.suit;
        }

        // returns true if the defending card legally defends the attacking
        public bool IsLegalDefense(Card attackingCard, Card defendingCard)
        {
            return (defendingCard.suit == attackingCard.suit &&
                    defendingCard.rank > attackingCard.rank) ||
                    (IsTrumpSuit(defendingCard) && (!IsTrumpSuit(attackingCard) ||
                    (IsTrumpSuit(attackingCard) && defendingCard.rank >
                    attackingCard.rank)));
        }

        // Called when the defender selects the defending card
        public MoveResult DefenderMove(int cardIndex)
        {
            Player defPlayer = GetPlayer(defendingPlayer);
            if (defPlayer.IsTaking())
            {
                return MoveResult.TookCards;
            }
            
            // wait for the attack
            if (GetPlayer(attackingPlayer).IsAttackersTurn())
            {
                return MoveResult.OutOfTurn;
            }

            int attackCardIndex = bout.GetAttackingCardsSize() - 1;

            Card attackingCard = bout.GetAttackingCard(attackCardIndex);
            Card defendingCard = defPlayer.GetCard(cardIndex);

            if (!IsLegalDefense(attackingCard, defendingCard))
            {
                return MoveResult.IllegalMove;
            }

            bout.AddDefendingCard(defendingCard);
            defPlayer.RemoveCardFromHand(defendingCard);

            // say defense finished to true by letting attacker to attack again
            GetPlayer(attackingPlayer).SetIsAttackersTurn(true);

            return MoveResult.OK;
        }
    }
}
