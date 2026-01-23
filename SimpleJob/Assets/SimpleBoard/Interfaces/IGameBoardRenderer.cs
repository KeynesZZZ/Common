using System;

namespace SimpleBoard.Interfaces
{
    public interface IGameBoardRenderer : IDisposable
    {
        void CreateGridTiles(int[,] data);
        void ResetGridTiles();
    }
}