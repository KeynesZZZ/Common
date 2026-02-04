using BingoShared.Models;

namespace BingoClient.Models
{
    /// <summary>
    /// 游戏数据单例 - 存储游戏运行时的全局数据
    /// </summary>
    public class GameData
    {
        public static GameData Instance { get; } = new();

        public string RoomId { get; set; }
        public string PlayerId { get; set; }
        public RoomData Room { get; set; }
        public System.Collections.Generic.List<BoardData> Boards { get; set; }
        public System.Collections.Generic.List<PlayerData> Players { get; set; }
    }

    /// <summary>
    /// 房间数据 - 包含房间的基本信息和玩家列表
    /// </summary>
    public class RoomData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int BingoCount { get; set; }
        public System.Collections.Generic.List<BoardData> Boards { get; set; }
        public System.Collections.Generic.List<PlayerData> Players { get; set; }
    }

    /// <summary>
    /// 玩家数据 - 包含玩家的基本信息和游戏状态
    /// </summary>
    public class PlayerData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public int Coins { get; set; }
        public int BingoCount { get; set; }
        public bool IsLocalPlayer { get; set; }
    }

    /// <summary>
    /// 棋盘数据 - 包含单个棋盘的所有格子信息
    /// </summary>
    public class BoardData
    {
        public string Id { get; set; }
        public string PlayerId { get; set; }
        public System.Collections.Generic.List<SlotData> Slots { get; set; }
        public int BoardIndex { get; set; }
    }

    /// <summary>
    /// 格子数据 - 包含单个格子的数字和状态
    /// </summary>
    public class SlotData
    {
        public int Index { get; set; }
        public int Number { get; set; }
        public bool IsMarked { get; set; }
        public bool HasPowerUp { get; set; }
        public PowerUpData PowerUp { get; set; }
    }

    /// <summary>
    /// 道具数据 - 包含道具类型和激活状态
    /// </summary>
    public class PowerUpData
    {
        public PowerUpType Type { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// 反馈数据 - 用于显示玩家的操作反馈
    /// </summary>
    public class FeedbackData
    {
        public FeedbackType Type { get; set; }
        public UnityEngine.Vector3 Position { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// 反馈类型 - 定义不同类型的操作反馈
    /// </summary>
    public enum FeedbackType
    {
        Perfect,
        Great,
        Miss,
        Bingo
    }
}