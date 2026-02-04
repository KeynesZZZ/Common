using BingoShared.Models;

namespace BingoClient.Models
{
    public class GameData
    {
        public static GameData Instance { get; } = new();

        public string RoomId { get; set; }
        public string PlayerId { get; set; }
        public RoomData Room { get; set; }
        public System.Collections.Generic.List<BoardData> Boards { get; set; }
        public System.Collections.Generic.List<PlayerData> Players { get; set; }
    }

    public class RoomData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int BingoCount { get; set; }
        public System.Collections.Generic.List<BoardData> Boards { get; set; }
        public System.Collections.Generic.List<PlayerData> Players { get; set; }
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
        public System.Collections.Generic.List<SlotData> Slots { get; set; }
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

    public class FeedbackData
    {
        public FeedbackType Type { get; set; }
        public UnityEngine.Vector3 Position { get; set; }
        public string Message { get; set; }
    }

    public enum FeedbackType
    {
        Perfect,
        Great,
        Miss,
        Bingo
    }
}