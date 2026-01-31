using System;
using Match3.Interfaces;
using UnityEngine;

namespace Match3Game.Interfaces
{
    public interface IGridTile : IGridSlotState, IDisposable
    {
        void SetActive(bool value);
        void SetWorldPosition(Vector3 worldPosition);
    }
}