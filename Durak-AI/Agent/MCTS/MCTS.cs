using Model.GameState;
using Model.PlayingCards;
using AIAgent.PolicyEvaluation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent
{
    public class MCTS : Agent 
    {
        private int limit;

        public MCTS(string name, int limit)
        {
            this.name = name;
            this.limit = limit;
        }
        
        //  Select or create a leaf node from the nodes already
        //  contained within the search tree(selection and expansion).
        private Node TreePolicy(Node node)
        {
            // while node is nonterminal
            while(!node.TerminalState())
            {
                if (node.IsNotFullyExpanded())
                {
                    return node.Expand();
                }
                else
                {
                    // balancer C = 1.41
                    node = node.BestChild(1.41);
                }
            }
            return node;
        }

        //  Play out the domain from a given non-terminal state
        //  to produce a value estimate(simulation).
        private int DefaultPolicy(GameView gameStateClone)
        {
            List<Agent> agents = new List<Agent>()
            {
                new GreedyAI("greedy"),
                new GreedyAI("greedy")
            };

            while (!gameStateClone.IsDone())
            {
                int turn = gameStateClone.Player();

                Card? action = agents[turn].Move(gameStateClone);
                gameStateClone.Apply(action);
            }
            return gameStateClone.Winner();
        }

        //  The simulation result is “backed up” (i.e.backpropagated) through
        //  the selected nodes to update their statistics.
        private void Backpropagation(Node nodeToExplore, int playoutResult)
        {
            Node tempNode = nodeToExplore;

            while(tempNode != null)
            {
                tempNode.IncrementPlayouts();
                // draw
                if (playoutResult == 0)
                {
                    tempNode.AddScore(0.5);
                } 
                else if (tempNode.GetGame().Player(false) == playoutResult)
                {
                    tempNode.AddScore(1.0);
                }
                tempNode = tempNode.GetParent();
            }
        }

        public override Card? Move(GameView gameView)
        {
            Tree tree = new Tree(gameView);
            Node rootNode = tree.GetRoot();

            int curr = 1;
            while (curr <= limit)
            {
                Node leafNode = TreePolicy(rootNode);
                int playoutResult = DefaultPolicy(leafNode.GetGame().Copy());
                Backpropagation(leafNode, playoutResult);

                curr++;
            }

            Node winnerNode = rootNode.BestChild(0);
            tree.SetRoot(rootNode);
            return winnerNode.GetLastAction();
        }
    }
}
