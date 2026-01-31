using System;

namespace Match3.Interfaces
{
    public interface IGameMode
    {
        event EventHandler Finished;
        void Activate();
    }
}