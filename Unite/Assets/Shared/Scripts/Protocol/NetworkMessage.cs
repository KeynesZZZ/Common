using System;

namespace BingoShared.Protocol
{
    public enum MessageType
    {
        JoinRoom = 1,
        JoinRoomResponse = 2,
        GameStart = 3,
        ClickSlot = 4,
        ClickSlotResponse = 5,
        CallNumber = 6,
        CallNumberResponse = 7,
        GameEnd = 8,
        GameEndResponse = 9,
        PlayerUpdate = 10,
        BingoAchieved = 11,
        PowerUpActivated = 12
    }

    public class NetworkMessage
    {
        public MessageType Type { get; set; }
        public string Data { get; set; }
        public long Timestamp { get; set; }
    }
}