using BingoShared.Models;
using System;
using System.Collections.Generic;

namespace BingoShared.Protocol
{
    public class JoinRoomResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public RoomData Room { get; set; }
    }

    public class ClickSlotResponse
    {
        public bool Success { get; set; }
        public int SlotIndex { get; set; }
        public bool IsMarked { get; set; }
        public bool HasPowerUp { get; set; }
        public PowerUpResult PowerUpResult { get; set; }
        public bool IsBingo { get; set; }
        public List<WinLine> WinLines { get; set; }
        public int RemainingBingo { get; set; }
    }

    public class CallNumberResponse
    {
        public int Number { get; set; }
        public List<int> CalledNumbers { get; set; }
    }

    public class GameEndResponse
    {
        public List<PlayerResult> Results { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class PowerUpResult
    {
        public PowerUpType Type { get; set; }
        public string Description { get; set; }
        public Reward Reward { get; set; }
        public int Coins { get; set; }
    }

    public class RoomData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int BingoCount { get; set; }
        public List<BoardData> Boards { get; set; }
        public List<PlayerData> Players { get; set; }
    }

    public class PlayerData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public int Coins { get; set; }
        public int BingoCount { get; set; }
        public bool IsLocalPlayer { get; set; }
    }

    public class BoardData
    {
        public string Id { get; set; }
        public string PlayerId { get; set; }
        public List<SlotData> Slots { get; set; }
        public int BoardIndex { get; set; }
    }

    public class SlotData
    {
        public int Index { get; set; }
        public int Number { get; set; }
        public bool IsMarked { get; set; }
        public bool HasPowerUp { get; set; }
        public PowerUpData PowerUp { get; set; }
    }

    public class PowerUpData
    {
        public PowerUpType Type { get; set; }
        public bool IsActive { get; set; }
    }
}