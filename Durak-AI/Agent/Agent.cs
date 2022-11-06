using System;

using Model.PlayingCards;
using Model.GameState;

namespace AIAgent
{
    public abstract class Agent
    {
        public string ?name;
        public abstract Card? Move(GameView gameView);
        public string? GetName() => name;
    }
}
