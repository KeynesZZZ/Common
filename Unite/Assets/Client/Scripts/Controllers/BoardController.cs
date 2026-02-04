using System.Collections.Generic;
using System.Threading.Tasks;
using BingoClient.Events;
using BingoClient.Models;

namespace BingoClient.Controllers
{
    public class BoardController
    {
        private readonly NetworkService _networkService;
        private readonly ClientEventBus _eventBus;
        private List<BoardData> _boards;
        private bool _interactionEnabled;

        public BoardController(NetworkService networkService, ClientEventBus eventBus)
        {
            _networkService = networkService;
            _eventBus = eventBus;
        }

        public void InitializeBoards(List<BoardData> boards)
        {
            _boards = boards;
            _eventBus.Publish(new ClientEvents.BoardsInitialized { Boards = boards });
        }

        public void EnableInteraction()
        {
            _interactionEnabled = true;
        }

        public async Task ClickSlotAsync(int boardIndex, int slotIndex)
        {
            if (!_interactionEnabled) return;

            var response = await _networkService.SendClickAsync(boardIndex, slotIndex);

            _eventBus.Publish(new ClientEvents.SlotClicked
            {
                BoardIndex = boardIndex,
                SlotIndex = slotIndex,
                IsMarked = response.IsMarked,
                HasPowerUp = response.HasPowerUp,
                PowerUpResult = response.PowerUpResult,
                IsBingo = response.IsBingo,
                WinLines = response.WinLines,
                RemainingBingo = response.RemainingBingo
            });
        }

        public void MarkSlot(int slotIndex, bool isMarked)
        {
            _eventBus.Publish(new ClientEvents.SlotMarked
            {
                SlotIndex = slotIndex,
                IsMarked = isMarked
            });
        }
    }
}