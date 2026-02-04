using System.Threading.Tasks;
using BingoClient.Events;
using BingoClient.Models;

namespace BingoClient.Controllers
{
    public class RoomController
    {
        private readonly NetworkService _networkService;
        private readonly ClientEventBus _eventBus;

        public RoomController(NetworkService networkService, ClientEventBus eventBus)
        {
            _networkService = networkService;
            _eventBus = eventBus;
        }

        public async Task JoinRoomAsync(string roomId, string playerId)
        {
            var response = await _networkService.SendJoinRoomAsync(roomId, playerId);

            if (response.Success)
            {
                GameData.Instance.RoomId = response.Room.Id;
                GameData.Instance.PlayerId = playerId;
                GameData.Instance.Room = response.Room;
                GameData.Instance.Boards = response.Room.Boards;
                GameData.Instance.Players = response.Room.Players;

                _eventBus.Publish(new ClientEvents.GameInitialized
                {
                    RoomData = response.Room
                });
            }
        }
    }
}