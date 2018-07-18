﻿using Boo.Lang;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public sealed class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly List<T> _pool = new List<T>();
        private readonly string _prefabLocation;
        private GameObject _prefab;

        public ObjectPool(string prefabLocation)
        {
            _prefabLocation = prefabLocation;
        }

        public T Create(Transform parent = null)
        {
            T newThing;
            if (_pool.Count == 0)
            {
                if (_prefab == null) _prefab = Resources.Load<GameObject>(_prefabLocation);
                GameObject newGameObject = Object.Instantiate(_prefab);
                newThing = newGameObject.GetComponent<T>();
                if(parent != null) newThing.transform.SetParent(parent, false);
                return newThing;
            }

            int lastElement = _pool.Count - 1;
            newThing = _pool[lastElement];
            newThing.gameObject.SetActive(true);
            _pool.RemoveAt(lastElement);
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