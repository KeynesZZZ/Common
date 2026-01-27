using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleBoard.Games.Bingo.Unity
{
    /// <summary>
    /// 对象池管理器 - 管理游戏对象的重用，减少 GC 压力
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        private static ObjectPool _instance;
        public static ObjectPool Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ObjectPool>();
                    if (_instance == null)
                    {
                        var poolObj = new GameObject("ObjectPool");
                        _instance = poolObj.AddComponent<ObjectPool>();
                        DontDestroyOnLoad(poolObj);
                    }
                }
                return _instance;
            }
        }

        [Header("Pool Settings")]
        [SerializeField] private int _defaultMaxPoolSize = 50;
        [SerializeField] private bool _autoExpand = false;

        // 对象池字典
        private Dictionary<GameObject, Queue<GameObject>> _objectPools = new Dictionary<GameObject, Queue<GameObject>>();
        private Dictionary<GameObject, int> _poolSizes = new Dictionary<GameObject, int>();

        /// <summary>
        /// 从对象池获取对象
        /// </summary>
        /// <param name="prefab">要获取的对象预制体</param>
        /// <param name="position">对象位置</param>
        /// <param name="rotation">对象旋转</param>
        /// <returns>获取的对象</returns>
        public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab cannot be null!");
                return null;
            }

            GameObject obj;

            // 检查对象池是否存在
            if (_objectPools.ContainsKey(prefab) && _objectPools[prefab].Count > 0)
            {
                // 从对象池获取
                obj = _objectPools[prefab].Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
            }
            else
            {
                // 创建新对象
                obj = Instantiate(prefab, position, rotation);
                obj.name = prefab.name;
            }

            return obj;
        }

        /// <summary>
        /// 将对象返回对象池
        /// </summary>
        /// <param name="prefab">对象的预制体</param>
        /// <param name="obj">要返回的对象</param>
        public void ReturnObject(GameObject prefab, GameObject obj)
        {
            if (prefab == null || obj == null)
            {
                Debug.LogError("Prefab or object cannot be null!");
                return;
            }

            // 确保对象池存在
            if (!_objectPools.ContainsKey(prefab))
            {
                _objectPools[prefab] = new Queue<GameObject>();
                _poolSizes[prefab] = _defaultMaxPoolSize;
            }

            // 检查对象池大小
            int maxSize = _poolSizes[prefab];
            if (_objectPools[prefab].Count < maxSize || _autoExpand)
            {
                // 重置对象状态
                obj.SetActive(false);
                obj.transform.parent = transform;
                obj.transform.localPosition = Vector3.zero;
                obj.transform.localRotation = Quaternion.identity;
                obj.transform.localScale = Vector3.one;

                // 清理组件状态
                var rigidbody = obj.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    rigidbody.velocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                }

                // 清理 Rigidbody2D 状态
                var rigidbody2D = obj.GetComponent<Rigidbody2D>();
                if (rigidbody2D != null)
                {
                    rigidbody2D.velocity = Vector2.zero;
                    rigidbody2D.angularVelocity = 0f;
                }

                // 清理粒子系统
                var particleSystem = obj.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Stop();
                    particleSystem.Clear();
                }

                // 返回对象池
                _objectPools[prefab].Enqueue(obj);
            }
            else
            {
                // 对象池已满，销毁对象
                Destroy(obj);
            }
        }

        /// <summary>
        /// 设置指定预制体的最大池大小
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="maxSize">最大池大小</param>
        public void SetPoolSize(GameObject prefab, int maxSize)
        {
            if (prefab == null)
                return;

            _poolSizes[prefab] = Mathf.Max(1, maxSize);
        }

        /// <summary>
        /// 预加载对象到池中
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="count">预加载数量</param>
        public void PreloadObjects(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0)
                return;

            // 确保对象池存在
            if (!_objectPools.ContainsKey(prefab))
            {
                _objectPools[prefab] = new Queue<GameObject>();
                _poolSizes[prefab] = _defaultMaxPoolSize;
            }

            // 预加载对象
            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(prefab, transform);
                obj.name = prefab.name;
                ReturnObject(prefab, obj);
            }
        }

        /// <summary>
        /// 清空指定预制体的对象池
        /// </summary>
        /// <param name="prefab">要清空的预制体</param>
        public void ClearPool(GameObject prefab)
        {
            if (prefab == null || !_objectPools.ContainsKey(prefab))
                return;

            foreach (var obj in _objectPools[prefab])
            {
                Destroy(obj);
            }
            _objectPools[prefab].Clear();
            _poolSizes.Remove(prefab);
        }

        /// <summary>
        /// 清空所有对象池
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var pool in _objectPools.Values)
            {
                foreach (var obj in pool)
                {
                    Destroy(obj);
                }
                pool.Clear();
            }
            _objectPools.Clear();
            _poolSizes.Clear();
        }

        /// <summary>
        /// 获取对象池信息
        /// </summary>
        /// <returns>对象池信息字符串</returns>
        public string GetPoolInfo()
        {
            string info = "Object Pool Info:\n";
            foreach (var kvp in _objectPools)
            {
                int maxSize = _poolSizes.TryGetValue(kvp.Key, out int size) ? size : _defaultMaxPoolSize;
                info += $"{kvp.Key.name}: {kvp.Value.Count}/{maxSize} objects\n";
            }
            return info;
        }

        /// <summary>
        /// 获取指定预制体的池大小
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <returns>当前池大小</returns>
        public int GetPoolCount(GameObject prefab)
        {
            if (prefab == null || !_objectPools.ContainsKey(prefab))
                return 0;

            return _objectPools[prefab].Count;
        }

        private void OnDestroy()
        {
            ClearAllPools();
            _instance = null;
        }
    }

    /// <summary>
    /// 对象池扩展方法
    /// </summary>
    public static class ObjectPoolExtensions
    {
        /// <summary>
        /// 从对象池获取对象
        /// </summary>
        /// <param name="prefab">要获取的对象预制体</param>
        /// <param name="position">对象位置</param>
        /// <param name="rotation">对象旋转</param>
        /// <returns>获取的对象</returns>
        public static GameObject GetFromPool(this GameObject prefab, Vector3 position = default, Quaternion rotation = default)
        {
            if (rotation == default)
            {
                rotation = Quaternion.identity;
            }
            return ObjectPool.Instance.GetObject(prefab, position, rotation);
        }

        /// <summary>
        /// 返回对象到对象池
        /// </summary>
        /// <param name="obj">要返回的对象</param>
        /// <param name="prefab">对象的预制体</param>
        public static void ReturnToPool(this GameObject obj, GameObject prefab)
        {
            ObjectPool.Instance.ReturnObject(prefab, obj);
        }

        /// <summary>
        /// 预加载对象到池中
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="count">预加载数量</param>
        public static void PreloadToPool(this GameObject prefab, int count)
        {
            ObjectPool.Instance.PreloadObjects(prefab, count);
        }

        /// <summary>
        /// 设置对象池大小
        /// </summary>
        /// <param name="prefab">预制体</param>
        /// <param name="maxSize">最大池大小</param>
        public static void SetPoolSize(this GameObject prefab, int maxSize)
        {
            ObjectPool.Instance.SetPoolSize(prefab, maxSize);
        }
    }
}
