using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers.Wilson_Score
{
    public interface IWilson
    {
        double CalcDenominator(int total_n);
        double CalcCenterProbability(double p, int total_n);
        double CalcStandardDeviation(double p, int total_n);
        (double, double) WilsonScore(double p, int total_n);
    }
}
