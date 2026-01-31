
namespace Match3.Interfaces
{
    public interface IGameBoardDataProvider<out TGridSlot> where TGridSlot : IGridSlot
    {
        TGridSlot[,] GetGameBoardSlots(int level);
    }
}