using Model.GameState;
using Model.PlayingCards;
using Model.DurakWrapper;

using System;
using static System.Math;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent
{
    public class MinimaxAI : Agent
    {
        private int maxDepth;
        private int totalGameStates;
        private int maxSearchedDepth;
        private bool debug;

        private HashSet<string> unique_states = new HashSet<string>();
        public MinimaxAI(string name, int depth, bool debug)
        {
            this.name = name;
            this.maxDepth = depth;
            this.debug = debug;
            this.totalGameStates = -1;
        }

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

        private int Evaluate(GameView gw)
        {
            int score = 0;

            if (gw.status == GameStatus.GameOver)
            {
                return gw.outcome;
            }

            return score;
        }

        private void SerializeGameState(GameView gw)
        {
            // prepare the options for serialization
            var options = new JsonSerializerOptions { IncludeFields = true };
            // serialize the object
            string json = JsonSerializer.Serialize(gw, options);
            Console.WriteLine(json);
            // store to hashset
            unique_states.Add(json);
        }

        // current minimax does always gives the card
        private int Minimax(GameView gw, int alpha, int beta, int depth, out Card? bestMove)
        {
            bestMove = null;
            totalGameStates += 1;

            SerializeGameState(gw);

            if (gw.status == GameStatus.GameOver || depth == maxDepth)
            {
                if (depth > maxSearchedDepth)
                {
                    maxSearchedDepth = depth;
                }
                return Evaluate(gw);
            }

            int bestVal = gw.plTurn == 0 ? int.MinValue : int.MaxValue;

            List<Card> possibleCards = gw.PossibleCards();
            // one more option is to take/pass
            List<Card?> possibleMoves = new List<Card?>(possibleCards);
            possibleMoves.Add(null);

            foreach(Card ?card in possibleMoves)
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

            if (debug)
            {
                Console.WriteLine($"Total states explored:    {totalGameStates}");
                Console.WriteLine($"Max search depth reached: {maxSearchedDepth}");
                Console.WriteLine($"Total unique states explored: {unique_states.Count}");
                unique_states.Clear();
                totalGameStates = 0;
                maxSearchedDepth = 0;
            }
            return bestMove;
        }
    }
}
