using System;
using SimpleBoard.Data;
using UnityEngine;

namespace SimpleBoard.Interfaces
{
    public interface IGridTile : IGridSlotState, IDisposable
    {
        void SetActive(bool value);
        void SetWorldPosition(Vector3 worldPosition);
    }
}