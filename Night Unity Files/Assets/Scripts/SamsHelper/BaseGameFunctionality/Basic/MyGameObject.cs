using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public class MyGameObject
    {
        public GameObject GameObject;
        public string Name { get; set; }
        private string _extendedName;
        public float Weight { get; set; }
        public Inventory Inventory { get; set; }
        public readonly GameObjectType Type;

        protected MyGameObject(string name, GameObjectType type, GameObject gameObject = null, float weight = 0, Inventory inventory = null)
        {
            Name = name;
            GameObject = gameObject;
            Weight = weight;
            Inventory = inventory;
            Type = type;
        }

        public string ExtendedName() => _extendedName ?? Name;
        public void SetExtendedName(string name) => _extendedName = name;

        public virtual InventoryUi CreateUi(Transform parent)
        {
            return new InventoryUi(this, parent);
        }
    }
}