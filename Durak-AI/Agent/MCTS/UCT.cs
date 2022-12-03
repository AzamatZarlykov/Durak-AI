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
        public static double UctValue(int turnParent, int turn, int parentVisit, double nodeWinScore, int nodeVisit, 
            double expParam, bool v = false)
        {
            if (nodeVisit == 0)
            {
                return int.MaxValue;
            }

            if (turn == turnParent)
            {
                if (v) Console.WriteLine("NO INVERT");
                return (double)nodeWinScore / (double)nodeVisit
                    + expParam * Math.Sqrt(2 * Math.Log(parentVisit) / (double)nodeVisit);
            }
            if (v) Console.WriteLine("INVERT");

            return 1 - (double)nodeWinScore / (double)nodeVisit
                + expParam * Math.Sqrt(2 * Math.Log(parentVisit) / (double)nodeVisit);
        }

        public static Node FindBestNodeWithUCT(Node node, double expParam)
        {
            // do not invert when the same player has 2 consecutive moves e.g
            // if the turn of the current node's parent is 1 and the current node's turn is 1
       
            var gameState = node.GetGame();
            int turn = gameState.Player(false);

            int parentVisit = node.GetTotalPlayout();

            return node.GetChildArray().MaxBy(cNode => UctValue(turn, cNode.GetGame().Player(false), parentVisit, cNode.GetTotalScore(),
                cNode.GetTotalPlayout(), expParam))!;
        }
    }
}
