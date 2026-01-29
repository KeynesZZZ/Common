using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleJob.Runtime;

namespace BlockBlast
{
    /// <summary>
    /// 消除行/列作业
    /// </summary>
    public class ClearLinesJob : Job
    {
        private readonly BlockGameBoard _board;
        private readonly List<int> _rows;
        private readonly List<int> _cols;

        public ClearLinesJob(BlockGameBoard board, List<int> rows, List<int> cols, int executionOrder)
            : base(executionOrder)
        {
            _board = board;
            _rows = rows;
            _cols = cols;
        }

        public override UniTask ExecuteAsync(CancellationToken token)
        {
            // 消除逻辑已经在BlockGameBoard.PlaceBlock中处理
            // 这里可以添加消除动画等待
            
            // await UniTask.Delay(300, cancellationToken: token);
            
            return UniTask.CompletedTask;
        }
    }
}
