using System;
using SimpleBoard.Enums;
using UnityEngine;

namespace SimpleBoard
{
    [Serializable]
    public class TileModel
    {
        [SerializeField] private TileGroup _group;
        [SerializeField] private GameObject _prefab;

        public TileGroup Group => _group;
        public GameObject Prefab => _prefab;
    }
}