using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Model.PlayingCards;
using AIAgent.PolicyEvaluation;
using Model.GameState;

namespace AIAgent
{
    public class Node
    {
        private GameView? game;

        private int playoutCount;   // number of playouts
        private double winscore;    // winscore of playouts
        private Node? parent;   // parent Node
        private Card? lastAction;
        private List<Node> childArray = new List<Node>();
        private List<Card?> allActions = new List<Card?>();

        // Construtors
        public Node() { }

        public Node(GameView game, Node parent, Card? action)
        {
            this.game = game;
            this.parent = parent;
            this.lastAction = action;
        }
        // MCTS related methods
        public bool IsNotFullyExpanded()
        {
            return allActions!.Count > 0;
        }

        public List<Card?> GetExpandedActions()
        {
            List<Card?> actions = new List<Card?>();

            foreach (Node childNode in GetChildArray())
            {
                actions.Add(childNode.GetLastAction());
            }

            return actions;
        }

        public Node GetChildWithMinScore()
        {
            return childArray.MinBy(child => child.winscore)!;
        }

        public Node Expand()
        {
            // Get untried action 
            Card? action = GetAllActions().First();
            GetAllActions().RemoveAt(0);

            GameView state = game!.Result(action);

            Node newNode = new Node(state, this, action);
            childArray.Add(newNode);

            return newNode;
        }

        public Node BestChild(double balancer)
        {
            return UCT.FindBestNodeWithUCT(this, balancer);
        }

        public bool TerminalState()
        {
            // in the terminal state when game is done
            return game!.IsDone();
        }

        // Getters
        public int GetTotalPlayout()
        {
            return playoutCount;
        }

        public double GetTotalScore()
        {
            return winscore;
        }

        public Node GetParent()
        {
            return parent!;
        }

        public GameView GetGame()
        {
            return game!;
        }

        public Card? GetLastAction()
        {
            return lastAction;
        }

        public List<Node> GetChildArray()
        {
            return childArray;
        }

        public List<Card?> GetAllActions()
        {
            return allActions;
        }

        // Setters
        public void IncrementPlayouts()
        {
            playoutCount++;
        }

        public void AddScore(double score)
        {
            winscore += score;
        }

        public void SetGameState(GameView g)
        {
            game = g;
        }

        public void SetAllActions(List<Card?> actions)
        {
            allActions = new List<Card?>(actions);
        }
    }
}
