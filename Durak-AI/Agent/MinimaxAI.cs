﻿using Model.GameState;
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
        private int depth;
        public MinimaxAI(int depth)
        {
            this.depth = depth;
        }

        // current minimax does always gives the card
        private int Minimax(GameView gw, int alpha, int beta, out Card? bestMove)
        {
            bestMove = null;

            if (gw.status == GameStatus.GameOver)
            {
                return gw.outcome;
            }

            int bestVal = gw.plTurn == 0 ? int.MinValue : int.MaxValue;

            foreach(Card card in gw.PossibleCards())
            {
                GameView gwCopy = gw.Copy();
                gwCopy.Move(card);
                int v = Minimax(gwCopy, alpha, beta, out Card? _);

                if (gw.plTurn == 0 ? v > bestVal : v < bestVal)
                {
                    bestVal = v;
                    bestMove = card;
                    if (gw.plTurn == 1)
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
            int alpha = int.MaxValue;
            int beta = int.MinValue;

            Minimax(gameView, alpha, beta, out Card? bestMove);

            return bestMove;
        }
    }
}
