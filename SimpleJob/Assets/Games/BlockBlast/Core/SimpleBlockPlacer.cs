using System.Threading;
using Cysharp.Threading.Tasks;
using Match3.Core;
using Match3.Interfaces;

namespace BlockBlast.Core
{
    public class SimpleBlockPlacer<TGridSlot> : IBlockPlacer<TGridSlot> where TGridSlot : IGridSlot
    {
        public bool CanPlaceBlock(IGameBoard<TGridSlot> gameBoard, GridPosition position, BlockShape blockShape)
        {
            foreach (var cell in blockShape.Cells)
            {
                var targetPosition = position + cell;
                if (!gameBoard.IsPositionOnBoard(targetPosition))
                {
                    return false;
                }
                if (gameBoard[targetPosition].HasItem)
                {
                    return false;
                }
            }
            return true;
        }

        public async UniTask PlaceBlockAsync(IGameBoard<TGridSlot> gameBoard, GridPosition position, BlockShape blockShape, int blockType, CancellationToken cancellationToken = default)
        {
            foreach (var cell in blockShape.Cells)
            {
                var targetPosition = position + cell;
                if (gameBoard.IsPositionOnBoard(targetPosition))
                {
                    var slot = gameBoard[targetPosition];
                    // 这里需要设置方块到格子中
                    // 实际实现中需要根据具体的IGridSlot实现来设置方块
                    // 例如：slot.SetItem(new BlockItem(blockType));
                }
            }
            await UniTask.Yield(cancellationToken);
        }
    }
}
