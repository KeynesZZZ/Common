using System;
using System.Collections.Generic;

namespace BingoShared.Models
{
    /// <summary>
    /// 房间数据 - 包含游戏房间的完整信息
    /// </summary>
    public class Room
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int MaxPlayers { get; set; }
        public List<Player> Players { get; set; }
        public List<Board> Boards { get; set; }
        public int BingoCount { get; set; }
        public GameState State { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    /// <summary>
    /// 玩家数据 - 包含玩家的基本信息和游戏状态
    /// </summary>
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Board> Boards { get; set; }
        public int Score { get; set; }
        public int Coins { get; set; }
        public int BingoCount { get; set; }
        public bool IsReady { get; set; }
    }

    /// <summary>
    /// 棋盘数据 - 包含单个棋盘的所有格子信息
    /// </summary>
    public class Board
    {
        public string Id { get; set; }
        public string PlayerId { get; set; }
        public List<Slot> Slots { get; set; }
        public int BoardIndex { get; set; }
    }

    /// <summary>
    /// 格子数据 - 包含单个格子的数字和状态
    /// </summary>
    public class Slot
    {
        public int Index { get; set; }
        public int Number { get; set; }
        public bool IsMarked { get; set; }
        public bool HasPowerUp { get; set; }
        public PowerUp PowerUp { get; set; }
    }

    /// <summary>
    /// 道具数据 - 包含道具类型和激活状态
    /// </summary>
    public class PowerUp
    {
        public PowerUpType Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

    /// <summary>
    /// 连线数据 - 包含连线类型和涉及的格子索引
    /// </summary>
    public class WinLine
    {
        public WinLineType Type { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int DiagonalIndex { get; set; }
        public List<int> SlotIndices { get; set; }
    }

    /// <summary>
    /// 玩家结果数据 - 包含游戏结束时的玩家排名和得分
    /// </summary>
    public class PlayerResult
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public int BingoCount { get; set; }
        public int Rank { get; set; }
    }

    /// <summary>
    /// 奖励数据 - 包含得分和金币奖励
    /// </summary>
    public class Reward
    {
        public int Score { get; set; }
        public int Coins { get; set; }
    }

    /// <summary>
    /// 游戏状态枚举 - 定义游戏的不同阶段
    /// </summary>
    public enum GameState
    {
        Waiting,
        Starting,
        Started,
        Ended
    }

    /// <summary>
    /// 道具类型枚举 - 定义游戏中可用的道具类型
    /// </summary>
    public enum PowerUpType
    {
        None,
        DoublePayout,
        DoubleDaub,
        Box,
        Coin
    }

    /// <summary>
    /// 连线类型枚举 - 定义不同的连线方式
    /// </summary>
    public enum WinLineType
    {
        Horizontal,
        Vertical,
        Diagonal,
        FourCorners,
        FullBoard
    }
}