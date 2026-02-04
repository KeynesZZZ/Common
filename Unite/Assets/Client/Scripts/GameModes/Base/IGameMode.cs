using BingoShared.Models;

namespace BingoClient.GameModes.Base
{
    public interface IGameMode
    {
        GameModeType ModeType { get; }
    }
}