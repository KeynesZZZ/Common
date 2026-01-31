using System;
using Match3Game.Enums;
using UnityEngine;

namespace Match3Game.Models
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