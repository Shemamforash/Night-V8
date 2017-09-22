using SamsHelper.BaseGameFunctionality.InventorySystem;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public class MyGameObject
    {
        public readonly GameObject GameObject;
        public string Name { get; set; }
        private string _extendedName;
        public float Weight { get; set; }
        private readonly Inventory _inventory;
        public readonly GameObjectType Type;

        protected MyGameObject(string name, GameObjectType type, GameObject gameObject = null, float weight = 0, Inventory inventory = null)
        {
            Name = name;
            GameObject = gameObject;
            Weight = weight;
            _inventory = inventory;
            Type = type;
        }

        public Inventory Inventory() => _inventory;
        public string ExtendedName() => _extendedName ?? Name;
        public void SetExtendedName(string name) => _extendedName = name;
    }
}