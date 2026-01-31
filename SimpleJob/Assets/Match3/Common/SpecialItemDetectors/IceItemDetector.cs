using System.Collections.Generic;
using Match3.Interfaces;
using Match3Game.Enums;
using Match3Game.Interfaces;

namespace Match3Game.SpecialItemDetectors
{
    public class IceItemDetector : ISpecialItemDetector<IUnityGridSlot>
    {
        public IEnumerable<IUnityGridSlot> GetSpecialItemGridSlots(IGameBoard<IUnityGridSlot> gameBoard,
            IUnityGridSlot gridSlot)
        {
            if (gridSlot.State.GroupId == (int)TileGroup.Ice)
            {
                yield return gridSlot;
            }
        }
    }
}