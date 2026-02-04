using BingoShared.Models;
using System.Threading.Tasks;

namespace BingoClient.GameModes.Base
{
    /// <summary>
    /// 游戏模式抽象基类 - 实现模板方法模式，定义游戏的基本生命周期
    /// 所有具体的游戏模式（经典模式、速度模式、道具模式）都继承此类
    /// </summary>
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