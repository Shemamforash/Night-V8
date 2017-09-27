using System;
using Facilitating.UI.Inventory;
using Game.World;
using SamsHelper.ReactiveUI.CustomTypes;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class InventoryResource : InventoryItem
    {
        private readonly MyInt _quantity = new MyInt(0);
        private readonly InventoryResourceType _inventoryResourceType;

        public InventoryResource(InventoryResourceType inventoryResourceType, float weight) : base(inventoryResourceType.ToString(), GameObjectType.Resource, weight)
        {
            _inventoryResourceType = inventoryResourceType;
        }

        public override bool Equals(object obj)
        {
            InventoryResource other = obj as InventoryResource;
            if (other != null)
            {
                return other.Name == Name;
            }
            return false;
        }

        public InventoryResourceType GetResourceType()
        {
            return _inventoryResourceType;
        }

        public override BaseInventoryUi CreateUi(Transform parent)
        {
            return new InventoryResourceUi(this, parent);
        }
        
        public void AddOnUpdate(Action<int> action)
        {
            _quantity.AddOnValueChange(action);
        }

        public int Decrement(int amount)
        {
            int previousQuantity = _quantity.Val;
            _quantity.Val = _quantity.Val - amount;
            int consumption = previousQuantity - _quantity.Val;
            return consumption;
        }

        public void Increment(int amount)
        {
            _quantity.Val += amount;
        }

        public override int Quantity()
        {
            return _quantity.Val;
        }

        public float GetWeight(int quantity)
        {
            return quantity * Weight;
        }
    }
}