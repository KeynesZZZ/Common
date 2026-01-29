using System.Threading;
using Cysharp.Threading.Tasks;
using SimpleJob;
using SimpleJob.Runtime;

namespace BlockBlast
{
    /// <summary>
    /// 更新分数作业
    /// </summary>
    public class UpdateScoreJob : Job
    {
        private readonly ScoreManager _scoreManager;
        private readonly int _rowsCleared;
        private readonly int _colsCleared;

        public UpdateScoreJob(ScoreManager scoreManager, int rowsCleared, int colsCleared, int executionOrder)
            : base(executionOrder)
        {
            _scoreManager = scoreManager;
            _rowsCleared = rowsCleared;
            _colsCleared = colsCleared;
        }

        public override UniTask ExecuteAsync(CancellationToken token)
        {
            // 更新分数
            _scoreManager.AddScore(_rowsCleared, _colsCleared);
            
            // 可以在这里添加分数动画等待
            // await UniTask.Delay(200, cancellationToken: token);
            
            return UniTask.CompletedTask;
        }
    }
}
