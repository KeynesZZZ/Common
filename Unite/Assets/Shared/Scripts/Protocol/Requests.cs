namespace BingoShared.Protocol
{
    public class JoinRoomRequest
    {
        public string RoomId { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
    }

    public class ClickSlotRequest
    {
        public string RoomId { get; set; }
        public string PlayerId { get; set; }
        public int BoardIndex { get; set; }
        public int SlotIndex { get; set; }
    }

    public class CallNumberRequest
    {
        public string RoomId { get; set; }
        public string PlayerId { get; set; }
    }
}