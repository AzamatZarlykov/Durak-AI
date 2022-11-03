using System;

using Model.PlayingCards;
using Model.GameState;

namespace AIAgent
{
    public abstract class Agent
    {
        public string ?name;
        public abstract Card? Move(GameView gameView, ref SavedState? savedState);
        public abstract void UpdateMemory(SavedState gameView, bool noTrumps);
        public string? GetName() => name;
    }
}
