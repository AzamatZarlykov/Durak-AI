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
using Helpers;

namespace AIAgent
{
    public class MinimaxAI : Agent
    {
        private int maxDepth;
        private int totalGameStates;
        private int maxSearchedDepth;
        private int samples;
        private bool debug;
        private string eval;

        // dictionary where the key is a tuple of stringified game state and depth and value is
        // the outcome of the game
        private Dictionary<(string, int), int> cache_states = new Dictionary<(string, int), int>();
        public MinimaxAI(string name, int depth, bool debug, string playout, int samples)
        {
            this.name = name;
            this.maxDepth = depth;
            this.debug = debug;
            this.totalGameStates = -1;
            this.samples = samples;
            this.eval = playout;

        }

        private int ConvertHandToValue(List<Card> hand, GameView gw)
        {
            const int TOTALCARDS = 9;
            int value = 0;

            foreach (Card card in hand)
            {
                if (gw.includeTrumps && card.suit == gw.trumpCard!.suit)
                    value += (int)card.rank + TOTALCARDS;
                else
                    value += (int)card.rank;
            }
            return value;
        }

        private int EvaluatePlayerHandToValue(GameView gw) =>
            ConvertHandToValue(gw.players[0].GetHand(), gw) -
            ConvertHandToValue(gw.players[1].GetHand(), gw);

        private int EvaluateHandSize(GameView gw)
        {
            int pHandSize = gw.players[0].GetNumberOfCards();
            int oHandSize = gw.players[1].GetNumberOfCards();

            // hand size matters in the late game
            pHandSize = (gw.isEarlyGame ? pHandSize : pHandSize * 15);
            oHandSize = (gw.isEarlyGame ? oHandSize : oHandSize * 15);

            return (pHandSize - oHandSize) * -1;
        }

        // If there a player has only one weakness it is guaranteed that this player will win
        private int EvaluateWeaknesses(GameView gw)
        {
            int value = 0;

            // stategy works if P attacking and O does not have any trump cards
            if (gw.turn == Turn.Attacking &&
                !gw.opponentHand.Exists(c => c.suit == gw.trumpCard?.suit))
            {
                List<Card?> possibleMoves = gw.Actions(excludePassTake: true);
                // cannot attack/defend
                if (possibleMoves.Count == 1 && possibleMoves[0] is null)
                {
                    return value;
                }

                List<Rank> weaknesses = Helper.GetWeaknesses(possibleMoves!, gw.GetOpponentCards());

                if (weaknesses.Count() == 1)
                {
                    value += 500;
                }
            }
            return value * gw.MMPlayer();
        }

        // Evaluates the current state of the game and returns its value
        // Assessment of the state: 1) value of the hand. 2) size of the hand
        // 3) in the end game, if a player has 1 weakness(also, opponent does not have trump cards)
        // there is a win for that player
        private int EvaluateState(GameView gw)
        {
            int score = 0;
            //1) get the value of the hand only if the hand sizes are the same
            if (gw.players[0].GetNumberOfCards() == gw.players[1].GetNumberOfCards())
            {
                score += EvaluatePlayerHandToValue(gw);
            }
            // 2) size of the hand: smaller -> better 
            score += EvaluateHandSize(gw);
            // 3) weaknesses 
            if (!gw.isEarlyGame)
            {
                score += EvaluateWeaknesses(gw);
            }
            return score;
        }

        private int Evaluate(GameView gw, int depth)
        {
            // simulate the game between 2 greedy AI agents. 
            // Based on the outcome return the score
            GameView innerGameView = gw.Copy();
            // initialize the agents
            List<Agent> agents = new List<Agent>()
            {
                new GreedyAI("greedy"),
                new GreedyAI("greedy")
            };

            // start the game simulation
            while (innerGameView.status == GameStatus.GameInProcess)
            {
                int turn = innerGameView.Player();

                Card? card = agents[turn].Move(innerGameView);
                innerGameView.Apply(card);
            }

            int result = innerGameView.MMWinner();
            int score = 1000 - depth;

            return result * score;
        }

        // current minimax does always gives the card
        private int Minimax(GameView gw, int alpha, int beta, int depth, out Card? bestMove)
        {
            bestMove = null;
            totalGameStates += 1;

            // serialize the object with fields
            string stringified_gamestate = gw.ToString();

            // if the game state was already explored then return its heurtic value
            if (cache_states.ContainsKey((stringified_gamestate, depth)))
            {
                return cache_states[(stringified_gamestate, depth)];
            }

            if (gw.status == GameStatus.GameOver || depth == maxDepth)
            {
                if (depth > maxSearchedDepth)
                {
                    maxSearchedDepth = depth;
                }

                if (gw.status == GameStatus.GameOver)
                {
                    return 1000 * gw.MMWinner();
                }

                return eval == "playout" ? Evaluate(gw, depth) : EvaluateState(gw);
            }

            int bestVal = gw.Player() == 0 ? int.MinValue : int.MaxValue;

            List<Card?> possibleMoves = gw.Actions(excludePassTake: false);

            foreach(Card ?card in possibleMoves)
            {
                GameView gwCopy = gw.Copy();
                gwCopy.Apply(card);
                int v = Minimax(gwCopy, alpha, beta, depth + 1, out Card? _);
                // if the game state was already explored then return its heurtic value
                if (!cache_states.ContainsKey((stringified_gamestate, depth)))
                {
                    // add to the cache the game state with its heurstic value
                    cache_states.Add((stringified_gamestate, depth), v);
                }

                if (gw.Player() == 0 ? v > bestVal : v < bestVal)
                {
                    bestVal = v;
                    bestMove = card;
                    if (gw.Player() == 0)
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

        private void ClosedPlay(GameView gw, int alpha, int beta, out Card? bestMove)
        {
            GameView sampleGame = gw.ShuffleCopy();
            Minimax(sampleGame, alpha, beta, 0, out bestMove);
        }

        private void ClosedEnvironmentMinimax(GameView gameView, int alpha, int beta, out Card? bestMove)
        {
            bestMove = null;

            if (!gameView.isEarlyGame)
            {
                ClosedPlay(gameView, alpha, beta, out bestMove);
                return;
            }

            // stores the frequency of the best moves out of n played moves
            Dictionary<Card, int> cache = new Dictionary<Card, int>();
            int passTakeTotal = 0;

            for (int i = 1; i <= samples; i++)
            {
                cache_states.Clear();
                ClosedPlay(gameView, alpha, beta, out bestMove);

                // best move can be null (PASS/TAKE). Cannot store null as a key to dict
                // thus keep track of the occurance
                if (bestMove is null)
                {
                    passTakeTotal += 1;
                }
                else
                {
                    if (cache.ContainsKey(bestMove))    
                    {
                        cache[bestMove] += 1;
                    }
                    else
                    {
                        cache[bestMove] = 1;
                    }
                }
            }
            if (cache.Count > 0)
            {
                // get the most frequent best move 
                bestMove = cache.MaxBy(kvp => kvp.Value).Key;

                // compare it to amout of pass/take instances
                if (passTakeTotal > cache[bestMove])
                {
                    bestMove = null;
                }
            }
        }

        public override Card? Move(GameView gameView)
        {
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            Card? bestMove;
            if (gameView.open)
            {
                Minimax(gameView, alpha, beta, 0, out bestMove);
            }
            else
            {
                ClosedEnvironmentMinimax(gameView, alpha, beta, out bestMove);
            }

            if (debug)
            {
                Console.WriteLine($"Max search depth reached: {maxSearchedDepth}");
                Console.WriteLine($"Total states explored:    {totalGameStates}");
                Console.WriteLine($"Total unique states explored: {cache_states.Count}");

                totalGameStates = 0;
                maxSearchedDepth = 0;
            }

            return bestMove;
        }
    }
}
