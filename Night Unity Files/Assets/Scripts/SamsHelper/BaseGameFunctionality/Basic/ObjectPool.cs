using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public sealed class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly List<T> _pool = new List<T>();
        private readonly string _prefabLocation;
        private GameObject _prefab;
        private Transform _poolParent;
        private readonly string _poolName;

        public ObjectPool(string poolName, string prefabLocation)
        {
            _prefabLocation = prefabLocation;
            _poolName = poolName;
        }

        public T Create()
        {
            if(_poolParent == null) _poolParent = new GameObject(_poolName).transform;
            T newThing;
            if (_pool.Count == 0)
            {
                if (_prefab == null) _prefab = Resources.Load<GameObject>(_prefabLocation);
                GameObject newGameObject = Object.Instantiate(_prefab);
                newThing = newGameObject.GetComponent<T>();
                newThing.transform.SetParent(_poolParent, false);
                return newThing;
            }

            newThing = _pool.RemoveLast();
            if (newThing == null) return Create();
            newThing.gameObject.SetActive(true);
            return newThing;
        }

        public void Return(T thing)
        {
            _pool.Add(thing);
            thing.gameObject.SetActive(false);
        }

        public void Dispose(T thing)
        {
            _pool.Remove(thing);
            Object.Destroy(thing.gameObject);
            if (Empty()) GameObject.Destroy(_poolParent);
        }

        public bool Empty()
        {
            return _pool.Count == 0;
        }

        public void Clear()
        {
            int poolSize = _pool.Count - 1;
            for (int i = poolSize; i >= 0; --i)
            {
                T pooledObject = _pool[i];
                Dispose(pooledObject);
            }
        }
    }
}