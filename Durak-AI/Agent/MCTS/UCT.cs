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
        public static double UctValue(int parentVisit, double nodeWinScore, int nodeVisit, double expParam)
        {
            if (nodeVisit == 0)
            {
                return int.MaxValue;
            }

            return 1 - (double)nodeWinScore / (double)nodeVisit
                + expParam * Math.Sqrt(2 * Math.Log(parentVisit) / (double)nodeVisit);
        }

        public static Node FindBestNodeWithUCT(Node node, double expParam)
        {
            int parentVisit = node.GetTotalPlayout();
            return node.GetChildArray().MaxBy(cNode => UctValue(parentVisit, cNode.GetTotalScore(),
                cNode.GetTotalPlayout(), expParam))!;
        }
    }
}
