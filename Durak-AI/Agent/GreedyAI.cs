using Model.GameState;
using Model.PlayingCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIAgent
{
    class GreedyAI : Agent
    {
        public GreedyAI() 
        {
            this.name = "GreedyAI";
        }

        public override Card? Move(GameView gameView)
        {
            throw new NotImplementedException();
        }
    }
}
