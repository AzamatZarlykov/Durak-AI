﻿using Model.GameState;
using Model.PlayingCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.DurakWrapper;

namespace AIAgent
{
    public class GreedyAI : Agent
    {
        public GreedyAI() 
        {
            this.name = "GreedyAI";
        }

        private List<Card> GetCardsWithoutTrump(List<Card> cards, Card trump)
        {
            List<Card> res = new List<Card>();

            foreach (Card card in cards)
            {
                if (card.suit != trump.suit) 
                {
                    res.Add(card);
                }
            }
            return res;
        }
        
        private Card GetLowestRank(List<Card> cards)
        {
            Card lowest = cards[0];
            for (int i = 1; i < cards.Count; i++)
            {
                Card card = cards[i];
                if (lowest.rank > card.rank)
                {
                    lowest = cards[i];
                }
            }
            return lowest;
        }

        // return true if deck is not empty, o/w false
        private bool EarlyGame(GameView gw)
        {
            return gw.deck.cardsLeft != 0;
        }

        // select lowest card that is not a trump. O/W pass/take
        private Card? GetCard(List<Card> cards, Card trump, GameView gw)
        {
            List<Card> noTrumpCards = GetCardsWithoutTrump(cards, trump);

            if (noTrumpCards.Count == 0)
            {
                if (EarlyGame(gw))
                {
                    // do not attack with trump card if there is no need
                    if (gw.turn == Turn.Attacking && gw.attackingCards.Count > 0)
                    {
                        return null;
                    }
                    // attack/defend o/w
                    return GetLowestRank(cards);
                }
                else
                {
                    return GetLowestRank(cards);
                }
            }
            return GetLowestRank(noTrumpCards);
        }

        /*
         1st Attack/Defend with smallest card if possible. No randomness
         */
        public override Card? Move(GameView gameView)
        {

            List<Card>? cards = gameView.PossibleCards();

            // cannot attack/defend
            if (cards is null)
            {
                return null;
            }

            return GetCard(cards, gameView.trumpCard, gameView);
        }
    }
}
