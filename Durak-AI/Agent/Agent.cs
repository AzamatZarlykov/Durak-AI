using System;

using Model.PlayingCards;
using Model.GameState;

namespace AIAgent
{
    public abstract class Agent
    {
        public abstract Card? Move(GameView gameView);
    }
}
