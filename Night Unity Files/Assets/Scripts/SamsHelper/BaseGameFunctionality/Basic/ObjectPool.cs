using System.Collections.Generic;
using SamsHelper.Libraries;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public sealed class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly List<T> _pool = new List<T>();
        private readonly List<T> _active = new List<T>();
        private readonly string _prefabLocation;
        private GameObject _prefab;
        private Transform _poolParent;
        private readonly string _poolName;

        public ObjectPool(string poolName, string prefabLocation)
        {
            _prefabLocation = prefabLocation;
            _poolName = poolName;
        }

        public ObjectPool(string prefabLocation)
        {
            _prefabLocation = prefabLocation;
        }

        private void CreatePoolParent()
        {
            if (_poolParent != null || _poolName == null) return;
            _poolParent = new GameObject(_poolName).transform;
            _poolParent.SetAsDynamicChild();
        }

        public T Create()
        {
            CreatePoolParent();
            return _pool.Count == 0 ? CreateNew() : Retrieve();
        }

        private T Retrieve()
        {
            T newThing = _pool.RemoveLast();
            if (newThing == null) return CreateNew();
            newThing.transform.position = Vector3.zero;
            _active.Add(newThing);
            newThing.gameObject.SetActive(true);
            return newThing;
        }

        private T CreateNew()
        {
            if (_prefab == null) _prefab = Resources.Load<GameObject>(_prefabLocation);
            GameObject newGameObject = Object.Instantiate(_prefab);
            T newThing = newGameObject.GetComponent<T>();
            newThing.transform.SetParent(_poolParent, false);
            newThing.transform.position = Vector3.zero;
            _active.Add(newThing);
            return newThing;
        }

        public void Return(T thing)
        {
            if (thing == null)
            {
                _active.RemoveAll(r => r == null);
                return;
            }

            _pool.Add(thing);
            _active.Remove(thing);
            thing.gameObject.SetActive(false);
        }

        public void Dispose(T thing)
        {
            _pool.Remove(thing);
            _active.Remove(thing);
            Object.Destroy(thing.gameObject);
            if (Empty() && _poolParent != null) GameObject.Destroy(_poolParent.gameObject);
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

        public List<T> Active()
        {
            return _active;
        }
    }
}