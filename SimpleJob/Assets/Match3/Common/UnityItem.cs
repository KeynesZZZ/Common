using Match3Game.Interfaces;
using UnityEngine;

namespace Match3Game
{
    public class UnityItem : MonoBehaviour, IUnityItem
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private bool _isDestroyed;
        public int Sn { get; private set; }
        public long UniqueID { get; private set; }
        
        public Transform Transform => transform;
        public SpriteRenderer SpriteRenderer => _spriteRenderer;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetWorldPosition(Vector3 worldPosition)
        {
            transform.position = worldPosition;
        }

        public Vector3 GetWorldPosition()
        {
            return transform.position;
        }

        public void SetScale(float value)
        {
            transform.localScale = new Vector3(value, value, value);
        }

        private void OnDestroy()
        {
            _isDestroyed = true;
        }

        public void Dispose()
        {
            if (_isDestroyed == false)
            {
                Destroy(gameObject);
            }
        }

        public async void SetSprite(long uniqueID, int sn, string icon)
        {
            UniqueID = uniqueID;
            Sn = sn;
            // var sprite = await Addressables.LoadAssetAsync<Sprite>(icon).ToUniTask();
            // _spriteRenderer.sprite = sprite;
        }
    }
}