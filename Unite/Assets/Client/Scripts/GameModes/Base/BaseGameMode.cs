using BingoShared.Models;
using System.Threading.Tasks;

namespace BingoClient.GameModes.Base
{
    public abstract class BaseGameMode : IGameMode
    {
        public abstract GameModeType ModeType { get; }

        public virtual Task InitializeAsync(string roomId)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnBeforeMarkAsync(string roomId, string playerId, int slotIndex)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnAfterMarkAsync(string roomId, string playerId, int slotIndex)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnBingoAchievedAsync(string roomId, string playerId, System.Collections.Generic.List<WinLine> winLines)
        {
            return Task.CompletedTask;
        }

        public virtual Task OnGameCompleteAsync(string roomId)
        {
            return Task.CompletedTask;
        }
    }
}