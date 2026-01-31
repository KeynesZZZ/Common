using Match3.Interfaces;
using System;

namespace Match3.Extensions
{
    public static class AppModeExtensions
    {
        public static void Deactivate(this IGameMode gameMode)
        {
            if (gameMode is IDeactivatable deactivatable)
            {
                deactivatable.Deactivate();
            }
        }

        public static void Dispose(this IGameMode gameMode)
        {
            if (gameMode is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}