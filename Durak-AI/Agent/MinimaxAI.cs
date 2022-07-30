using Model.GameState;
using Model.PlayingCards;
using Model.DurakWrapper;

using System;
using static System.Math;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent
{
    public class MinimaxAI : Agent
    {
        private int maxDepth;
        public MinimaxAI(string name, int depth)
        {
            this.name = name;
            this.maxDepth = depth;
        }

        /*        private int DefendAttack(List<Card> oHand, List<Card> pHand, GameView gw)
                {
                    var oHandCopy = new List<Card>(oHand);

                    foreach (Card oCard in oHand)
                    {
                        var dCards = gw.GetDefendingCards(oCard);

                    }
                }*/

        private int AttackingEval(GameView gw, List<Card> oHand)
        {
            var possibleCards = gw.PossibleCards();
            var weaknesses = Helper.GetWeaknesses(possibleCards, oHand);

            if (weaknesses.Count == 1)
            {
                return 100;
            }
            return 0;
        }

/*
    - Early Game
        - Attacking
        - Defending
            - if can defend any attack of the opponent = +15
            - else if can defend only non-trump cards  = +10
    - End Game
        - Attacking
            - if opponent does not have trumps and p has 1 weakness = +100 (winner)
            - else if p has more trump cards than non trump cards and 
              oHand is at least the same size as pHand = +10
            - else if p has 
        - Defending
            - if can defend any attack of the opponent = +20
 */
        private int Evaluate(GameView gw)
        {
            int score = 0;
            var oHand = gw.opponentHand;

            if (gw.isEarlyGame)
            {
                if (gw.turn == Turn.Attacking)
                {

                }
                else
                {

                }
            
            }
            else
            {
                if (gw.turn == Turn.Attacking && !oHand.Exists(c => c.suit == gw.trumpSuit))
                {
                    score += AttackingEval(gw, oHand);
                }
            }

            return score;
        }

        // current minimax does always gives the card
        private int Minimax(GameView gw, int alpha, int beta, int depth, out Card? bestMove)
        {
            bestMove = null;

            if (gw.status == GameStatus.GameOver)
            {
                return gw.outcome;
            }

            if (depth == maxDepth)
            {
                // heuristc estimate 
                return Evaluate(gw);
            }

            int bestVal = gw.plTurn == 0 ? int.MinValue : int.MaxValue;

            foreach(Card card in gw.PossibleCards())
            {
                GameView gwCopy = gw.Copy();
                gwCopy.Move(card);
                int v = Minimax(gwCopy, alpha, beta, depth + 1, out Card? _);

                if (gw.plTurn == 0 ? v > bestVal : v < bestVal)
                {
                    bestVal = v;
                    bestMove = card;
                    if (gw.plTurn == 0)
                    {
                        if (v >= beta)
                        {
                            return v;
                        }
                        alpha = Max(alpha, v);
                    } else
                    {
                        if (v <= alpha)
                        {
                            return v;
                        }
                        beta = Min(beta, v);
                    }
                }
            }
            return bestVal;
        }

        public override Card? Move(GameView gameView)
        {
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            Minimax(gameView, alpha, beta, 0, out Card? bestMove);

            // Console.WriteLine("#################THE BEST MOVE: ", bestMove);
                
            return bestMove;
        }
    }
}
