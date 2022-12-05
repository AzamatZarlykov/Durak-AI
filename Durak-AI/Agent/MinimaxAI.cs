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
        private bool debug;

        // dictionary where the key is a tuple of stringified game state and depth and value is
        // the outcome of the game
        private Dictionary<(string, int), int> cache_states = new Dictionary<(string, int), int>();
        public MinimaxAI(string name, int depth, bool debug)
        {
            this.name = name;
            this.maxDepth = depth;
            this.debug = debug;
            this.totalGameStates = -1;
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

        // Returns the value of the player's hand. E.g card with rank 6 has value 6 Ace has
        // value 14. Trump cards, if allowed, continue the sequence 6 trump has value 15
        private int EvaluatePlayersHandsToValue(GameView gw)
        {
            int oValue = 0;

            // converting hands of the player has the turn
            int pValue = ConvertHandToValue(gw.playerHand, gw);

            // converting opponent's hand
            if (gw.open)
                oValue = ConvertHandToValue(gw.opponentHand, gw);
            else
            {
                var oSeenCards = gw.GetOpponentCards();
                oValue = ConvertHandToValue(oSeenCards, gw);

                // add the average of the cards that are not seen from opponent hand and deck
                var hiddenOpponentCards = gw.opponentHand.Where(c => !c.GetSeen());
                var deckCards = gw.deck.GetCards();

                int oppHiddenCardsValue = ConvertHandToValue(hiddenOpponentCards.ToList(), gw);
                int deckValue = ConvertHandToValue(deckCards.ToList(), gw);

                oValue = (oppHiddenCardsValue + deckValue) / 
                    (hiddenOpponentCards.Count() + deckCards.Count());
                
            }
/*            Console.WriteLine($"P hand: {Formatter.toString(gw.playerHand)}");
            Console.WriteLine($"O hand: {Formatter.toString(gw.opponentHand)}");

            Console.WriteLine($"P player hand value: {pValue}");
            Console.WriteLine($"O player hand value: {oValue}");*/
            return pValue - oValue;
        }

        private int EvaluateHandSize(GameView gw)
        {
            int pHandSize = gw.playerHand.Count();
            int oHandSize = gw.opponentHand.Count();

            int dif = pHandSize - oHandSize;

            if (!gw.isEarlyGame)
            {
                // in the end game difference in hand size matters more than in the early game
                dif = dif * 5;
            }
            // having more cards is worse thus reverse
            return dif * (-1);
        }

        // If there a player has only one weakness it is guaranteed that this player will win
        private int EvaluateWeaknesses(GameView gw)
        {
            int value = 0;
            // stategy works if P attacking and O does not have any trump cards
            if (gw.turn == Turn.Attacking && !gw.opponentHand.Exists(c => c.suit == gw.trumpCard?.suit))
            {
                List<Card?> possibleMoves = gw.Actions(excludePassTake: true);
                List<Rank> weaknesses = Helper.GetWeaknesses(possibleMoves!, gw.GetOpponentCards());
                
                if (weaknesses.Count() == 1)
                {
                    value += 500; 
                }
            }
            return value * gw.Player(false);
        }

        // Evaluates the current state of the game and returns its value
        // Assessment of the state: 1) value of the hand. 2) size of the hand
        // 3) in the end game, if a player has 1 weakness(also, opponent does not have trump cards)
        // there is a win for that player
        private int EvaluateState(GameView gw)
        {
            int score = 0;

            // 1) get the value of the hand
            score += EvaluatePlayersHandsToValue(gw);

            //Console.WriteLine($"Score after hand value: {score}");

            // 2) size of the hand: smaller -> better 
            score += EvaluateHandSize(gw);

            //Console.WriteLine($"Score after hand size: {score}");
            if (!gw.open)
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

            int result = innerGameView.outcome;
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
                    return 1000 * gw.outcome;
                }

                return EvaluateState(gw);
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

        private void ClosedEnvironmentMinimax(GameView gameView, int alpha, int beta, out Card? bestMove)
        {
            bestMove = null;
            // sample the game state: take the deck (except the trump) and non seen cards
            // and shuffle them. After, redistribute cards back to player and the deck
            // This state should be played out within minimax n times (n=10) and select the
            // most common card option
            int n = 20;
            // stores the frequency of the best moves out of n played moves
            Dictionary<Card, int> cache = new Dictionary<Card, int>();
            int passTakeTotal = 0;

            for (int i = 1; i <= n; i++)
            {
                cache_states.Clear();
                GameView sampleGame = gameView.ShuffleCopy();
                Minimax(sampleGame, alpha, beta, 0, out bestMove);

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
