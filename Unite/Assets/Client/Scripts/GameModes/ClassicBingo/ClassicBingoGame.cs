using BingoClient.GameModes.Base;
using BingoShared.Models;

namespace BingoClient.GameModes.ClassicBingo
{
    public class ClassicBingoGame : BaseGameMode
    {
        public override GameModeType ModeType => GameModeType.ClassicBingo;

        public ClassicBingoGame()
        {
        }
    }
}