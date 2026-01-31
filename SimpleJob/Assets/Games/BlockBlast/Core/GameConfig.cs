using Match3.Core;
using Match3.Interfaces;

namespace BlockBlast.Core
{
    public class BlockGameConfig<TGridSlot> : GameConfig<TGridSlot> where TGridSlot : IGridSlot
    {
        public IBlockPlacer<TGridSlot> BlockPlacer { get; set; }
        public IBalancedBlockSelector BlockSelector { get; set; }
    }
}
