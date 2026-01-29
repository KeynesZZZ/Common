using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleBoard.Core;
using SimpleJob;
using SimpleJob.Runtime;

namespace BlockBlast
{
    /// <summary>
    /// 放置方块作业
    /// </summary>
    public class PlaceBlockJob : Job
    {
        private readonly BlockGameBoard _board;
        private readonly BlockData _block;
        private readonly GridPosition _position;

        public PlaceBlockJob(BlockGameBoard board, BlockData block, GridPosition position, int executionOrder)
            : base(executionOrder)
        {
            _board = board;
            _block = block;
            _position = position;
        }

        public override UniTask ExecuteAsync(CancellationToken token)
        {
            // 同步执行放置逻辑
            _board.PlaceBlock(_block, _position);
            
            // 可以在这里添加动画等待
            // await UniTask.Delay(100, cancellationToken: token);
            
            return UniTask.CompletedTask;
        }
    }
}
