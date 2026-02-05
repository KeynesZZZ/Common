using System.Collections.Generic;
using UnityEngine;

namespace BlockBlast.Utils
{
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private Queue<T> pool;
        private T prefab;
        private Transform parent;

        public ObjectPool(T prefab, int initialSize, Transform parent = null)
        {
            this.prefab = prefab;
            this.parent = parent;
            pool = new Queue<T>(initialSize);

            for (int i = 0; i < initialSize; i++)
            {
                CreateInstance();
            }
        }

        private T CreateInstance()
        {
            T instance = Object.Instantiate(prefab, parent);
            instance.gameObject.SetActive(false);
            pool.Enqueue(instance);
            return instance;
        }

        public T Get()
        {
            if (pool.Count == 0)
            {
                CreateInstance();
            }

            T instance = pool.Dequeue();
            instance.gameObject.SetActive(true);
            return instance;
        }

        public void Return(T instance)
        {
            instance.gameObject.SetActive(false);
            pool.Enqueue(instance);
        }

        public void Clear()
        {
            pool.Clear();
        }
    }
}
