using UnityEngine;
using System.Collections;

namespace BingoClient.Services
{
    public class AnimationService : MonoBehaviour
    {
        private static AnimationService _instance;

        public static AnimationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<AnimationService>();
                }
                return _instance;
            }
        }

        public void PlayDaubEffect(Vector3 position)
        {
            var effect = ObjectPool.Instance.Get("DaubEffect");
            effect.transform.position = position;
            effect.SetActive(true);

            var particleSystem = effect.GetComponent<ParticleSystem>();
            particleSystem.Play();

            StartCoroutine(ReturnToPoolAfterDelay(effect, particleSystem.main.duration));
        }

        public void PlayNumberBallAnimation(Transform ballTransform)
        {
            ballTransform.DOScale(Vector3.one * 1.2f, 0.2f).SetEase(Ease.OutBack);
            ballTransform.DORotate(Vector3.forward * 360f, 0.5f).SetEase(Ease.OutBack);
        }

        public void PlayWinLineAnimation(List<Transform> slotTransforms)
        {
            for (int i = 0; i < slotTransforms.Count; i++)
            {
                var delay = i * 0.1f;
                slotTransforms[i].DOScale(Vector3.one * 1.3f, 0.3f).SetDelay(delay).SetEase(Ease.OutBack);
                slotTransforms[i].DORotate(Vector3.forward * 180f, 0.5f).SetDelay(delay).SetEase(Ease.OutBack);
            }
        }

        private IEnumerator ReturnToPoolAfterDelay(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            obj.SetActive(false);
            ObjectPool.Instance.Return(obj);
        }
    }

    public class ObjectPool : MonoBehaviour
    {
        private static ObjectPool _instance;
        private readonly Dictionary<string, Queue<GameObject>> _pools = new();
        private readonly Dictionary<string, GameObject> _prefabs = new();

        public static ObjectPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ObjectPool>();
                }
                return _instance;
            }
        }

        public void RegisterPrefab(string key, GameObject prefab)
        {
            _prefabs[key] = prefab;
            if (!_pools.ContainsKey(key))
            {
                _pools[key] = new Queue<GameObject>();
            }
        }

        public GameObject Get(string key)
        {
            if (_pools.ContainsKey(key) && _pools[key].Count > 0)
            {
                var obj = _pools[key].Dequeue();
                obj.SetActive(true);
                return obj;
            }

            if (_prefabs.ContainsKey(key))
            {
                return Instantiate(_prefabs[key]);
            }

            return null;
        }

        public void Return(GameObject obj)
        {
            obj.SetActive(false);
            var key = obj.name.Replace("(Clone)", "");
            if (!_pools.ContainsKey(key))
            {
                _pools[key] = new Queue<GameObject>();
            }
            _pools[key].Enqueue(obj);
        }
    }
}