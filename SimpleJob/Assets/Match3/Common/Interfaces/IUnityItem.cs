using System;
using UnityEngine;

namespace Match3Game.Interfaces
{
    public interface IUnityItem : IDisposable
    {
        long UniqueID { get; }
        int Sn { get; }
        Transform Transform { get; }
        SpriteRenderer SpriteRenderer { get; }

        void Show();
        void Hide();
        void SetSprite(long uniqueID, int sn, string icon);
        void SetWorldPosition(Vector3 worldPosition);
        Vector3 GetWorldPosition();
        void SetScale(float value);
    }
}