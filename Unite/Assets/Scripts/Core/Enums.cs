using UnityEngine;

namespace BingoGame.Core
{
    /// <summary>
    /// 游戏模式枚举
    /// </summary>
    public enum GameMode
    {
        /// <summary>经典数字Bingo</summary>
        ClassicBingo,
        /// <summary>割草Bingo</summary>
        HarvestBingo
    }

    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        /// <summary>无状态</summary>
        None,
        /// <summary>初始化中</summary>
        Initializing,
        /// <summary>准备就绪</summary>
        Ready,
        /// <summary>游戏中</summary>
        Playing,
        /// <summary>暂停</summary>
        Paused,
        /// <summary>检查胜利条件</summary>
        CheckingWin,
        /// <summary>胜利</summary>
        Win,
        /// <summary>失败</summary>
        Lose,
        /// <summary>游戏结束</summary>
        GameOver
    }

    /// <summary>
    /// 草地颜色枚举（割草Bingo使用）
    /// </summary>
    public enum GrassColor
    {
        /// <summary>红色</summary>
        Red,
        /// <summary>蓝色</summary>
        Blue,
        /// <summary>绿色</summary>
        Green,
        /// <summary>黄色</summary>
        Yellow,
        /// <summary>紫色</summary>
        Purple
    }

    /// <summary>
    /// 道具类型枚举
    /// </summary>
    public enum ItemType
    {
        /// <summary>无</summary>
        None,
        /// <summary>即时Bingo：自动标记一个数字</summary>
        InstantBingo,
        /// <summary>双倍积分：获得双倍分数</summary>
        DoublePoints,
        /// <summary>额外时间：延长游戏时间</summary>
        ExtraTime,
        /// <summary>免费标记：免费标记一个格子</summary>
        FreeDaub,
        /// <summary>宝箱：随机获得奖励</summary>
        TreasureChest,
        /// <summary>金币加成：增加金币获取</summary>
        CoinBoost,
        /// <summary>魔法标记：标记时触发特效</summary>
        MagicDaub,
        /// <summary>幸运数字：指定数字更容易出现</summary>
        LuckyLumber,
        /// <summary>护盾：防止负面效果</summary>
        Shield,
        /// <summary>加速：加快数字抽取速度</summary>
        SpeedUp,
        /// <summary>汽油：给对应颜色的小车加油（割草Bingo）</summary>
        Gasoline,
        /// <summary>钥匙：直接收割对应颜色的整列（割草Bingo）</summary>
        Key,
        /// <summary>自动收割：自动收割一个格子（割草Bingo）</summary>
        AutoHarvest
    }

    /// <summary>
    /// 道具稀有度枚举
    /// </summary>
    public enum ItemRarity
    {
        /// <summary>普通</summary>
        Common,
        /// <summary>稀有</summary>
        Rare,
        /// <summary>史诗</summary>
        Epic,
        /// <summary>传说</summary>
        Legendary
    }

    /// <summary>
    /// 道具目标枚举
    /// </summary>
    public enum ItemTarget
    {
        /// <summary>只对自己生效</summary>
        Self,
        /// <summary>对所有玩家生效</summary>
        AllPlayers,
        /// <summary>对随机玩家生效</summary>
        RandomPlayer,
        /// <summary>对对手生效</summary>
        Opponents
    }

    /// <summary>
    /// 音效类型枚举
    /// </summary>
    public enum SoundType
    {
        /// <summary>格子标记</summary>
        CellMark,
        /// <summary>数字呼叫</summary>
        NumberCall,
        /// <summary>胜利</summary>
        Win,
        /// <summary>失败</summary>
        Lose,
        /// <summary>按钮点击</summary>
        ButtonClick,
        /// <summary>割草</summary>
        Harvest,
        /// <summary>小车加油</summary>
        CarRefuel,
        /// <summary>整列收割</summary>
        ColumnHarvest
    }

    /// <summary>
    /// 音乐类型枚举
    /// </summary>
    public enum MusicType
    {
        /// <summary>背景音乐</summary>
        Background,
        /// <summary>胜利音乐</summary>
        Win,
        /// <summary>失败音乐</summary>
        Lose
    }
}
