using System;

namespace Match3.Interfaces
{
    public interface IGameBoardRenderer : IDisposable
    {
        void CreateGridTiles(int[,] data);
        void ResetGridTiles();
    }
}