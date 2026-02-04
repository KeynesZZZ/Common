using UnityEngine;
using Cysharp.Threading.Tasks;
using BingoGame.Core;

namespace BingoGame.Services
{
    /// <summary>
    /// 动画服务接口
    /// </summary>
    public interface IAnimationService
    {
        UniTask PlayCellMarkAnimationAsync(Transform cellTransform);
        UniTask PlayNumberCallAnimationAsync(int number);
        UniTask PlayWinAnimationAsync(System.Collections.Generic.List<Transform> winningCells);
        UniTask PlayLoseAnimationAsync();
        UniTask PlayShakeAnimationAsync(Transform target);
        UniTask PlayHarvestAnimationAsync(int row, int col);
        UniTask PlayCarRefuelAnimationAsync(GrassColor color);
        UniTask PlayColumnHarvestAnimationAsync(int col);
        void StopAllAnimations();
    }

    /// <summary>
    /// 动画服务单例
    /// 使用DOTween实现动画效果
    /// </summary>
    public class AnimationService : MonoBehaviour, IAnimationService
    {
        private static AnimationService instance;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static AnimationService Instance => instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// 播放单元格标记动画
        /// </summary>
        /// <param name="cellTransform">单元格Transform</param>
        public async UniTask PlayCellMarkAnimationAsync(Transform cellTransform)
        {
            if (cellTransform == null)
            {
                Debug.LogWarning("单元格Transform为空");
                return;
            }

            await cellTransform.DOScale(1.2f, 0.1f).SetEase(Ease.OutBack).ToUniTask();
            await cellTransform.DOScale(1f, 0.1f).SetEase(Ease.InBack).ToUniTask();
        }

        /// <summary>
        /// 播放数字呼叫动画
        /// </summary>
        /// <param name="number">呼叫的数字</param>
        public async UniTask PlayNumberCallAnimationAsync(int number)
        {
            Debug.Log($"播放数字呼叫动画: {number}");
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
        }

        /// <summary>
        /// 播放胜利动画
        /// </summary>
        /// <param name="winningCells">胜利单元格列表</param>
        public async UniTask PlayWinAnimationAsync(System.Collections.Generic.List<Transform> winningCells)
        {
            if (winningCells == null || winningCells.Count == 0)
            {
                Debug.LogWarning("胜利单元格列表为空");
                return;
            }

            Debug.Log($"播放胜利动画，单元格数量: {winningCells.Count}");

            var sequence = DG.Tweening.DOTween.Sequence();

            foreach (var cell in winningCells)
            {
                if (cell != null)
                {
                    sequence.Join(cell.DOScale(1.3f, 0.3f).SetEase(Ease.OutBack));
                    sequence.Join(cell.DORotate(new Vector3(0, 360, 0), 0.5f, DG.Tweening.RotateMode.FastBeyond360));
                }
            }

            await sequence.ToUniTask();

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

            foreach (var cell in winningCells)
            {
                if (cell != null)
                {
                    await cell.DOScale(1f, 0.2f).SetEase(Ease.InBack).ToUniTask();
                    cell.DORotate(Vector3.zero, 0.2f);
);
                }
            }
        }

        /// <summary>
        /// 播放失败动画
        /// </summary>
        public async UniTask PlayLoseAnimationAsync()
        {
            Debug.Log("播放失败动画");
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        }

        /// <summary>
        /// 播放抖动动画
        /// </summary>
        /// <param name="target">目标Transform</param>
        public async UniTask PlayShakeAnimationAsync(Transform target)
        {
            if (target == null)
            {
                Debug.LogWarning("目标Transform为空");
                return;
            }

            var originalPosition = target.localPosition;
            await target.DOShakePosition(0.5f, 0.2f, 10).ToUniTask();
            target.localPosition = originalPosition;
        }

        /// <summary>
        /// 播放割草动画
        /// </summary>
        /// <param name="row">行索引</param>
        /// <param name="col">列索引</param>
        public async UniTask PlayHarvestAnimationAsync(int row, int col)
        {
            Debug.Log($"播放割草动画: ({row}, {col})");
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        }

        /// <summary>
        /// 播放小车加油动画
        /// </summary>
        /// <param name="color">颜色</param>
        public async UniTask PlayCarRefuelAnimationAsync(GrassColor color)
        {
            Debug.Log($"播放小车加油动画: {color}");
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f));
        }

        /// <summary>
        /// 播放整列收割动画
        /// </summary>
        /// <param name="col">列索引</param>
        public async UniTask PlayColumnHarvestAnimationAsync(int col)
        {
            Debug.Log($"播放整列收割动画: {col}");
            await UniTask.Delay(TimeSpan.FromSeconds(0.4f));
        }

        /// <summary>
        /// 停止所有动画
        /// </summary>
        public void StopAllAnimations()
        {
            DG.Tweening.DOTween.KillAll();
            Debug.Log("停止所有动画");
        }
    }
}
