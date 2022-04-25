using System;

using Model.PlayingCards;
using Model.GameState;

namespace AIAgent
{
    public abstract class Agent
    {
        protected string? name;

        public abstract Card? Move(GameView gameView);

        public string? GetName()
        {
            return name;
        }
    }
}
