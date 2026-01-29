using SimpleBoard.Core;
using SimpleBoard.Data;
using SimpleBoard.Interfaces;
using UnityEngine;

namespace BlockBlast
{
    /// <summary>
    /// Block Blast 方块槽位接口
    /// 扩展 IUnityGridSlot 添加方块特定功能
    /// </summary>
    public interface IBlockGridSlot : IUnityGridSlot
    {
        /// <summary>
        /// 方块类型标识
        /// </summary>
        int BlockType { get; }
        
        /// <summary>
        /// 方块颜色
        /// </summary>
        Color BlockColor { get; }
        
        /// <summary>
        /// 是否包含方块
        /// </summary>
        bool HasBlock { get; }
        
        /// <summary>
        /// 设置方块
        /// </summary>
        void SetBlock(int blockType, Color color);
        
        /// <summary>
        /// 移除方块
        /// </summary>
        void RemoveBlock();
    }
}
