using System;
using System.Collections.Generic;

namespace BingoShared.Models
{
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

    public class Board
    {
        public string Id { get; set; }
        public string PlayerId { get; set; }
        public List<Slot> Slots { get; set; }
        public int BoardIndex { get; set; }
    }

    public class Slot
    {
        public int Index { get; set; }
        public int Number { get; set; }
        public bool IsMarked { get; set; }
        public bool HasPowerUp { get; set; }
        public PowerUp PowerUp { get; set; }
    }

    public class PowerUp
    {
        public PowerUpType Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime ExpiryTime { get; set; }
    }

    public class WinLine
    {
        public WinLineType Type { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
        public int DiagonalIndex { get; set; }
        public List<int> SlotIndices { get; set; }
    }

    public class PlayerResult
    {
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public int BingoCount { get; set; }
        public int Rank { get; set; }
    }

    public class Reward
    {
        public int Score { get; set; }
        public int Coins { get; set; }
    }

    public enum GameState
    {
        Waiting,
        Starting,
        Started,
        Ended
    }

    public enum PowerUpType
    {
        None,
        DoublePayout,
        DoubleDaub,
        Box,
        Coin
    }

    public enum WinLineType
    {
        Horizontal,
        Vertical,
        Diagonal,
        FourCorners,
        FullBoard
    }
}