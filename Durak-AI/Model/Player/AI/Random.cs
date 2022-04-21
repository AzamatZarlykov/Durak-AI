using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.GameState;
using Model.PlayingCards;

namespace Model.GamePlayer
{
    public class RandomAI : Player
    {
        // for random card generation
        private Random random = new Random();
        public RandomAI()
        {

        }

        // Checks if the passed card can be used to attack in the current bout
        private bool IsAttackPossible(Card card, List<Card> attCards, List<Card> defCards)
        {
            foreach (Card c in attCards)
            {
                if (card.rank == c.rank)
                {
                    return true;
                }
            }

            foreach (Card c in defCards)
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
        private List<Card> GenerateListOfAttackingCards(List<Card> attCards, List<Card> defCards)
        {
            List<Card> result = new List<Card>();

            foreach (Card card in hand)
            {
                if (IsAttackPossible(card, attCards, defCards))
                {
                    result.Add(card);
                }
            }

            return result;
        }

        public override Card Attack(GameView gameView)
        {
            List<Card> possibleAttackCards = GenerateListOfAttackingCards(
                gameView.attackingCards,
                gameView.defendingCards
            );

            int size = possibleAttackCards.Count;
            int attackCardIndex = random.Next(0, size);

            Card attackCard = hand[attackCardIndex];
            RemoveCardFromHand(attackCard);

            return attackCard;
        }

        /// <summary>
        /// This method generates the list of cards that can defend the attacking card
        /// 
        /// It iterates over the hand and checks if the card can legally defend from attacking card
        /// </summary>
        /// <param name="attackingCard"></param>
        /// <param name="trump"></param>
        /// <returns></returns>
        private List<Card> GenerateListofDefendingCards(Card attackingCard, Card trump)
        {
            List<Card> result = new List<Card>();

            foreach (Card card in hand)
            {
                if (IsLegalDefense(attackingCard, card, trump))
                {
                    result.Add(card);
                }
            }

            return result;
        }

        public override Card Defend(GameView gameView)
        {
            // get the attacking card from the view
            Card attackingCard = gameView.attackingCards[^1];
            // create a set of cards that possibly can defend
            List<Card> possibleDefenseCards = GenerateListofDefendingCards(
                attackingCard, 
                gameView.trumpCard
            );

            int size = possibleDefenseCards.Count;
            int defendingCardIndex = random.Next(0, size);

            Card defendCard = hand[defendingCardIndex];
            RemoveCardFromHand(defendCard);


            return defendCard;
        }

        public override bool CanAttack(GameView gameView)
        {
            if(gameView.attackingCards.Count == 0)
            {
                return true;
            }

            foreach (Card card in hand)
            {
                return gameView.attackingCards.Exists(c => c.rank == card.rank) ||
                       gameView.defendingCards.Exists(c => c.rank == card.rank);
            }

            return false;
        }

        public override bool CanDefend(GameView gameView)
        {
            Card attackingCard = gameView.attackingCards[^1];

            foreach (Card defendingCard in hand)
            {
                if (IsLegalDefense(attackingCard, defendingCard, gameView.trumpCard))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
