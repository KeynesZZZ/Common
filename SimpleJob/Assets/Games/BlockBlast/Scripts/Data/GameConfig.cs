using UnityEngine;

namespace BlockBlast
{
    /// <summary>
    /// 游戏配置类
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "BlockBlast/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("游戏板设置")]
        [Tooltip("游戏板宽度")]
        public int BoardWidth = 8;
        
        [Tooltip("游戏板高度")]
        public int BoardHeight = 8;

        [Header("分数设置")]
        [Tooltip("消除一行/列的基础分数")]
        public int BaseScore = 100;
        
        [Tooltip("同时消除行和列的额外奖励")]
        public int ComboBonus = 200;
        
        [Tooltip("连击倍数增长")]
        public float ComboMultiplier = 0.5f;

        [Header("游戏设置")]
        [Tooltip("每回合提供的方块数量")]
        public int BlocksPerTurn = 3;
        
        [Tooltip("是否显示放置预览")]
        public bool ShowPlacementPreview = true;

        [Header("视觉效果")]
        [Tooltip("放置动画时长")]
        public float PlaceAnimationDuration = 0.2f;
        
        [Tooltip("消除动画时长")]
        public float ClearAnimationDuration = 0.3f;
        
        [Tooltip("分数动画时长")]
        public float ScoreAnimationDuration = 0.5f;

        /// <summary>
        /// 获取默认配置
        /// </summary>
        public static GameConfig Default => CreateInstance<GameConfig>();
    }
}
