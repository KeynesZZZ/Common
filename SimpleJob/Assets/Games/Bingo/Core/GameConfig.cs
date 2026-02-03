using UnityEngine; using Bingo.Interfaces;

namespace Bingo.Core
{
    [CreateAssetMenu(fileName = "BingoGameConfig", menuName = "Bingo/GameConfig")]
    public class GameConfig : ScriptableObject, IBingoGameConfig
    {
        [Header("Bingo 游戏配置")]
        [SerializeField]
        private int cardSize = 5;
        public int CardSize => cardSize;

        [SerializeField]
        private float callNumberDelay = 1.5f;
        public float CallNumberDelay => callNumberDelay;

        [SerializeField]
        private int maxPlayers = 4;
        public int MaxPlayers => maxPlayers;
    }
}
