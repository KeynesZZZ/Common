using BingoClient.GameModes.Base;
using BingoShared.Models;
using System.Threading.Tasks;

namespace BingoClient.GameModes.PowerUpBingo
{
    public class PowerUpBingoGame : BaseGameMode
    {
        public override GameModeType ModeType => GameModeType.PowerUpBingo;

        public PowerUpBingoGame()
        {
        }

        public override async Task OnAfterMarkAsync(string roomId, string playerId, int slotIndex)
        {
            await base.OnAfterMarkAsync(roomId, playerId, slotIndex);

            var gameData = BingoClient.Models.GameData.Instance;
            var board = gameData.Boards.FirstOrDefault();
            if (board != null && slotIndex >= 0 && slotIndex < board.Slots.Count)
            {
                var slot = board.Slots[slotIndex];
                if (slot.HasPowerUp && slot.PowerUp != null && slot.PowerUp.IsActive)
                {
                    await ActivatePowerUpAsync(slot.PowerUp.Type);
                }
            }
        }

        private async Task ActivatePowerUpAsync(PowerUpType powerUpType)
        {
            var eventBus = BingoClient.Utilities.ServiceLocator.GetService<BingoClient.Events.ClientEventBus>();
            
            var powerUpResult = new BingoShared.Protocol.PowerUpResult
            {
                Type = powerUpType,
                Description = GetPowerUpDescription(powerUpType)
            };

            eventBus.Publish(new BingoClient.Events.ClientEvents.PowerUpActivated
            {
                PowerUpResult = powerUpResult
            });
        }

        private string GetPowerUpDescription(PowerUpType type)
        {
            return type switch
            {
                PowerUpType.DoublePayout => "双倍收益已激活",
                PowerUpType.DoubleDaub => "双倍点击反馈",
                PowerUpType.Box => "随机开出奖励",
                PowerUpType.Coin => "获得金币",
                _ => "道具已激活"
            };
        }
    }
}