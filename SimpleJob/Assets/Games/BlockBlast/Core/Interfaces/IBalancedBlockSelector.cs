using System.Collections.Generic;

namespace BlockBlast.Core
{
    public interface IBalancedBlockSelector
    {
        List<BlockShape> SelectBlocks(int count = 3);
    }
}
