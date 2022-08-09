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
        public int Seed { get; set; }
        public bool Verbose { get; set; }
        public bool Debug { get; set; }

        public bool OpenWorld { get; set; }
        public string[] Agents { get; set; } = new string[2];

    }
}
