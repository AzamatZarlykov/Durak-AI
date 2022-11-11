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
        private bool openWorld;

        // dictionary where the key is a tuple of stringified game state and depth and value is
        // the outcome of the game
        private Dictionary<(string, int), int> cache_states = new Dictionary<(string, int), int>();
        public MinimaxAI(string name, int depth, bool debug, bool openWorld)
        {
            this.name = name;
            this.maxDepth = depth;
            this.debug = debug;
            this.totalGameStates = -1;
            this.openWorld = openWorld;
        }

        private int Evaluate(GameView gw, int depth)
        {
            if (gw.status == GameStatus.GameOver)
            {
                return gw.outcome;
            }

            // simulate the game between 2 greedy AI agents. 
            // Based on the outcome return the score
            Durak innerGame = new Durak(gw);
            // initialize the agents
            List<Agent> agents = new List<Agent>()
            {
                new GreedyAI("greedy"),
                new GreedyAI("greedy")
            };

            // start the game simulation
            while (innerGame.gameStatus == GameStatus.GameInProcess)
            {
                int turn = innerGame.GetTurn();

                Card? card = agents[turn].Move(new GameView(innerGame, turn, openWorld));
                innerGame.Move(card);
            }

            int result = innerGame.GetGameResult() / 1000;
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
                return Evaluate(gw, depth);
            }

            int bestVal = gw.plTurn == 0 ? int.MinValue : int.MaxValue;

            List<Card?> possibleMoves = gw.PossibleMoves(excludePass: false);

            foreach(Card ?card in possibleMoves)
            {
                GameView gwCopy = gw.Copy();
                gwCopy.Move(card);
                int v = Minimax(gwCopy, alpha, beta, depth + 1, out Card? _);
                // if the game state was already explored then return its heurtic value
                if (!cache_states.ContainsKey((stringified_gamestate, depth)))
                {
                    // add to the cache the game state with its heurstic value
                    cache_states.Add((stringified_gamestate, depth), v);
                }

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

        private void ClosedEnvironmentMinimax(GameView gameView, int alpha, int beta, out Card? bestMove)
        {
            bestMove = null;
            // sample the game state: take the deck (except the trump) and non seen cards
            // and shuffle them. After, redistribute cards back to player and the deck
            // This state should be played out within minimax n times (n=10) and select the
            // most common card option
            int n = 3;
            // stores the frequency of the best moves out of n played moves
            Dictionary<Card, int> cache = new Dictionary<Card, int>();
            int passTakeTotal = 0;

            Durak sampleGame = new Durak(gameView, false); ;
            for (int i = 1; i <= n; i++)
            {
                sampleGame.Sample();
                GameView sampleGameView = new GameView(
                    sampleGame, gameView.GetAgentIndex(), gameView.open
                );

                Minimax(sampleGameView, alpha, beta, 0, out bestMove);

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

            // print the dictionary 
/*            foreach (KeyValuePair<Card, int> kvp in cache)
            {
                //textBox3.Text += ("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                Console.WriteLine("Key = {0}, Value = {1}", kvp.Key, kvp.Value);
            }*/
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
