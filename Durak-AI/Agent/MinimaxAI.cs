using Model.GameState;
using Model.PlayingCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent
{
    public class MinimaxAI : Agent
    {
        private int depth;
        public MinimaxAI(int depth)
        {
            this.depth = depth;
        }

        private int Minimax(int depth)
        {
            int best = 0;

            return best;
        }

        public override Card? Move(GameView gameView)
        {

            throw new NotImplementedException();
        }
    }
}
