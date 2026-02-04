using UnityEngine;
using BingoGame.Core;
using BingoGame.Core.Models;

namespace BingoGame.GameModes.HarvestBingo
{
    /// <summary>
    /// 割草Bingo单元格
    /// </summary>
    public class GrassCell : GameCell
    {
        /// <summary>
        /// 草地颜色
        /// </summary>
        public GrassColor Color { get; set; }

        /// <summary>
        /// 是否已收割
        /// </summary>
        public bool IsHarvested { get; set; }

        /// <summary>
        /// 生长进度（0-1）
        /// </summary>
        public float GrowthProgress { get; set; }

        /// <summary>
        /// 是否有小车（有汽油道具）
        /// </summary>
        public bool HasCar { get; set; }

        /// <summary>
        /// 是否有钥匙（有钥匙道具）
        /// </summary>
        public bool HasKey { get; set; }

        /// <summary>
        /// 是否已完成（收割状态）
        /// </summary>
        public override bool IsCompleted
        {
            get => IsHarvested;
            set => IsHarvested = value;
        }
    }
}
