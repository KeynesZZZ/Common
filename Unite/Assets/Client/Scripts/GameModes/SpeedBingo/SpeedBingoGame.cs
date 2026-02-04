using BingoClient.GameModes.Base;
using BingoShared.Models;
using System.Threading.Tasks;

namespace BingoClient.GameModes.SpeedBingo
{
    /// <summary>
    /// 速度 Bingo 游戏模式 - 在经典模式基础上增加速度奖励
    /// 玩家每次标记格子都会获得 1.5 倍的分数奖励
    /// </summary>
    public class SpeedBingoGame : BaseGameMode
    {
        public override GameModeType ModeType => GameModeType.SpeedBingo;
        private float _speedMultiplier = 1.5f;

        public SpeedBingoGame()
        {
        }

        public override async Task OnAfterMarkAsync(string roomId, string playerId, int slotIndex)
        {
            await base.OnAfterMarkAsync(roomId, playerId, slotIndex);
            
            var gameData = BingoClient.Models.GameData.Instance;
            var player = gameData.Players.FirstOrDefault(p => p.Id == playerId);
            if (player != null)
            {
                player.Score += (int)(10 * _speedMultiplier);
            }
        }
    }
}