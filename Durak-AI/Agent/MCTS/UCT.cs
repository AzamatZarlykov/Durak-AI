using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent.PolicyEvaluation
{
    // !!! HAS TO BE CHANGED FOR DURAK MODEL
    public class UCT
    {
        public static double UctValue(Node parent, Node child, double expParam, bool v = false)
        {
            int parentPlayouts = parent.GetTotalPlayout();
            int parentTurn = parent.GetGame().Player(false);

            int childPlayouts = child.GetTotalPlayout();
            int childTurn = child.GetGame().Player(false);
            double childWinscore = child.GetTotalScore();

            if (childPlayouts == 0)
            {
                return int.MaxValue;
            }

            if (parentTurn == childTurn)
            {
                if (v) Console.WriteLine("NO INVERT");
                return (double)childWinscore / (double)childPlayouts
                    + expParam * Math.Sqrt(2 * Math.Log(parentPlayouts) / (double)childPlayouts);
            }
            if (v) Console.WriteLine("INVERT");

            return 1 - (double)childWinscore / (double)childPlayouts
                + expParam * Math.Sqrt(2 * Math.Log(parentPlayouts) / (double)childPlayouts);
        }

        public static Node FindBestNodeWithUCT(Node node, double expParam)
        {
            // do not invert when the same player has 2 consecutive moves e.g
            // if the turn of the current node's parent is 1 and the current node's turn is 1
            return node.GetChildArray().MaxBy(c => UctValue(node, c, expParam))!;
        }
    }
}
