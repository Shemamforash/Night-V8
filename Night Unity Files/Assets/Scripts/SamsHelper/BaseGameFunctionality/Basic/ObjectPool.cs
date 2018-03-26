using Boo.Lang;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public sealed class ObjectPool<T> where T : MonoBehaviour
    {
        private GameObject _prefab;
        private readonly List<T> _pool = new List<T>();
        private readonly string _prefabLocation;
        
        public ObjectPool(string prefabLocation)
        {
            _prefabLocation = prefabLocation;
        }

        public T Create()
        {
            T newThing;
            if (_pool.Count == 0)
            {
                if(_prefab == null) _prefab = Resources.Load<GameObject>(_prefabLocation);
                GameObject newGameObject = GameObject.Instantiate(_prefab);
                newThing = newGameObject.GetComponent<T>();
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
            GameObject.Destroy(thing.gameObject);
        }
    }
}