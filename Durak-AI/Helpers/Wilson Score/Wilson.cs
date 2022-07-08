using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.Wilson_Score
{
    /// <summary>
    /// This class calculates the Wilson score interval with 98% confidence interval
    /// </summary>
    public class Wilson
    {
        private double z;
        public Wilson()
        {
            z = 2.326; // 98% confidence interval
        }

        private double CalcDenominator(int total_n)
        {
            // 1 + z**2/n
            return 1 + z * z / total_n;
        }

        private double CalcCenterProbability(double p, int total_n)
        {
            // p + z*z / (2*n)
            return p + z * z / (2 * total_n);
        }

        private double CalcStandardDeviation(double p, int total_n)
        {
            // sqrt(( p*(1 - p) + z*z / 4*n ) / n)
            return Math.Sqrt( (p * (1 - p) + z * z / (4 * total_n)) / total_n );
        }

        public (double, double) WilsonScore(double p, int total_n)
        {
            double denominator = CalcDenominator(total_n);
            double center_prob = CalcCenterProbability(p, total_n);

            double sd = CalcStandardDeviation(p, total_n);

            double lower_bound = (center_prob - z * sd) / denominator;
            double upper_bound = (center_prob + z * sd) / denominator;

            return (lower_bound, upper_bound);
        }
    }
}
