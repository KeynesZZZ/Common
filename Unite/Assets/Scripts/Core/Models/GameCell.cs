using UnityEngine;

namespace BingoGame.Core.Models
{
    /// <summary>
    /// 游戏单元格抽象基类
    /// 所有玩法的单元格都继承此类
    /// </summary>
    public abstract class GameCell
    {
        /// <summary>
        /// 单元格在棋盘中的位置
        /// </summary>
        public Vector2Int Position { get; set; }

        /// <summary>
        /// 单元格是否已完成（标记/收割）
        /// </summary>
        public abstract bool IsCompleted { get; set; }
    }
}
