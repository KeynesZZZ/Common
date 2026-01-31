using System.Threading;
using Cysharp.Threading.Tasks;

namespace Match3.Interfaces
{
    public interface IItemSwapper<in TGridSlot> where TGridSlot : IGridSlot
    {
        UniTask SwapItemsAsync(TGridSlot gridSlot1, TGridSlot gridSlot2, CancellationToken cancellationToken = default);
    }
}