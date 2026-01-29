using System.Threading;
using Cysharp.Threading.Tasks;

namespace SimpleJob.Runtime
{
    public interface IJob
    {
        int ExecutionOrder { get; }

        UniTask ExecuteAsync(CancellationToken cancellationToken = default);
    }
}