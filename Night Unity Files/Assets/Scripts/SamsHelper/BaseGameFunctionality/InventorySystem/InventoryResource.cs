using System;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class InventoryResource : InventoryItem
    {
        private readonly MyValue _quantity = new MyValue();
        private readonly InventoryResourceType _inventoryResourceType;

        public InventoryResource(InventoryResourceType inventoryResourceType, float weight) : base(inventoryResourceType.ToString(), GameObjectType.Resource, weight)
        {
            _inventoryResourceType = inventoryResourceType;
        }

        public override float Quantity()
        {
            return _quantity.GetCurrentValue();
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

        public override InventoryUi CreateUi(Transform parent)
        {
            InventoryUi ui = base.CreateUi(parent);
            ui.SetRightTextCallback(() => "x" + Quantity());
            return ui;
        }
        
        public void AddOnUpdate(Action<MyValue> action)
        {
            _quantity.AddOnValueChange(action);
        }

        public float Decrement(float amount)
        {
            float previousQuantity = _quantity.GetCurrentValue();
            _quantity.SetCurrentValue(_quantity.GetCurrentValue() - amount);
            float consumption = previousQuantity - _quantity.GetCurrentValue();
            return consumption;
        }

        public void Increment(float amount)
        {
            _quantity.SetCurrentValue(_quantity.GetCurrentValue() + amount);
        }

        public float GetWeight(float quantity)
        {
            return quantity * Weight;
        }
    }
}