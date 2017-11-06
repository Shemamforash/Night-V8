using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public abstract class MyGameObject
    {
        private GameObject _gameObject;
        public string Name { get; set; }
        private string _extendedName;
        public float Weight { get; set; }
        public Inventory ParentInventory { get; set; }
        public readonly GameObjectType Type;

        protected MyGameObject(string name, GameObjectType type, float weight = 0, Inventory parentInventory = null)
        {
            Name = name;
            Weight = weight;
            ParentInventory = parentInventory;
            Type = type;
        }

        public virtual void SetGameObject(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public GameObject GetGameObject()
        {
            return _gameObject;
        }

        public string ExtendedName() => _extendedName ?? Name;
        public void SetExtendedName(string name) => _extendedName = name;

        public virtual ViewParent CreateUi(Transform parent)
        {
            return new SimpleView(this, parent);
        }
    }
}