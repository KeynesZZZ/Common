using System.Threading;
using Cysharp.Threading.Tasks;

namespace Job
{
    public interface IJob
    {
        int ExecutionOrder { get; }

        UniTask ExecuteAsync(CancellationToken cancellationToken = default);
    }
}