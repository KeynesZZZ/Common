using UnityEngine;
using Cysharp.Threading.Tasks;

namespace BingoGame.GameModes
{
    /// <summary>
    /// 游戏模式接口
    /// 定义所有玩法必须实现的基本功能
    /// </summary>
    public interface IGameMode
    {
        /// <summary>
        /// 玩法类型
        /// </summary>
        GameMode ModeType { get; }

        /// <summary>
        /// 玩法名称
        /// </summary>
        string ModeName { get; }

        /// <summary>
        /// 玩法描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 初始化游戏模式
        /// </summary>
        UniTask InitializeAsync();

        /// <summary>
        /// 开始游戏
        /// </summary>
        UniTask StartGameAsync();

        /// <summary>
        /// 暂停游戏
        /// </summary>
        UniTask PauseGameAsync();

        /// <summary>
        /// 恢复游戏
        /// </summary>
        UniTask ResumeGameAsync();

        /// <summary>
        /// 结束游戏
        /// </summary>
        UniTask EndGameAsync();

        /// <summary>
        /// 重置游戏
        /// </summary>
        UniTask ResetGameAsync();

        /// <summary>
        /// 检查胜利条件
        /// </summary>
        /// <returns>是否胜利</returns>
        bool CheckWinCondition();

        /// <summary>
        /// 处理玩家输入
        /// </summary>
        /// <param name="position">点击位置</param>
        UniTask HandlePlayerInputAsync(Vector2Int position);
    }
}
