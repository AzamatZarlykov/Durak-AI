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
            int score = 0;

            if (gw.status == GameStatus.GameOver)
            {
                return gw.outcome;
            }

            // simulate the game between 2 greedy AI agents. 
            // Based on the outcome return the score
/*            Durak inner_game = new Durak(gw);
            // initialize the agents
            List<Agent> agents = new List<Agent>()
            {
                new GreedyAI("greedy"),
                new GreedyAI("greedy")
            };

            // start the game simulation
            while (inner_game.gameStatus == GameStatus.GameInProcess)
            {
                int turn = inner_game.GetTurn();

                Card? card = agents[turn].Move(new GameView(inner_game, turn, openWorld));
                inner_game.Move(card);
            }
*/
            
            return score;
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
                //Console.WriteLine("Encountered State");
                //Console.WriteLine(stringified_gamestate);
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

            List<Card?> possibleMoves = gw.PossibleCards();

            foreach(Card ?card in possibleMoves)
            {
                GameView gwCopy = gw.Copy();
                gwCopy.Move(card);
                int v = Minimax(gwCopy, alpha, beta, depth + 1, out Card? _);
                // if the game state was already explored then return its heurtic value
                if (!cache_states.ContainsKey((stringified_gamestate, depth)))
                {
                    //Console.WriteLine("Storing the state");
                    //Console.WriteLine(stringified_gamestate);
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
        
        public override Card? Move(GameView gameView)
        {
            int alpha = int.MinValue;
            int beta = int.MaxValue;

            Minimax(gameView, alpha, beta, 0, out Card? bestMove);

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
