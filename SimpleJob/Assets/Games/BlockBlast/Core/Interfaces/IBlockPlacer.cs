using System.Threading;
using Cysharp.Threading.Tasks;
using Match3.Core;
using Match3.Interfaces;

namespace BlockBlast.Core
{
    public interface IBlockPlacer<TGridSlot> where TGridSlot : IGridSlot
    {
        bool CanPlaceBlock(IGameBoard<TGridSlot> gameBoard, GridPosition position, BlockShape blockShape);
        UniTask PlaceBlockAsync(IGameBoard<TGridSlot> gameBoard, GridPosition position, BlockShape blockShape, int blockType, CancellationToken cancellationToken = default);
    }
}
