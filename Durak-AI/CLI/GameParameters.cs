using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AIAgent;

namespace CLI
{
    internal class GameParameters
    {
        public int NumberOfGames { get; set; }
        public int StartingRank { get; set; }
        public List<Agent> Agents { get; set; } = new List<Agent>();
        public bool Verbose { get; set; }

    }
}
