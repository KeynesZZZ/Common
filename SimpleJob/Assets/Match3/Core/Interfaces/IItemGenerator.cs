using System;

namespace Match3.Interfaces
{
    public interface IItemGenerator : IDisposable
    {
        void CreateItems(int capacity);
    }
}