using System;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class InventoryResource : InventoryItem
    {
        private readonly InventoryResourceType _inventoryResourceType;
        private readonly Number _quantity = new Number();

        public InventoryResource(InventoryResourceType inventoryResourceType, float weight) : base(inventoryResourceType.ToString(), GameObjectType.Resource, weight)
        {
            _inventoryResourceType = inventoryResourceType;
        }

        public override float Quantity()
        {
            return _quantity.CurrentValue();
        }

        public override bool Equals(object obj)
        {
            InventoryResource other = obj as InventoryResource;
            if (other != null) return other.Name == Name;
            return false;
        }

        public InventoryResourceType GetResourceType()
        {
            return _inventoryResourceType;
        }

        public override ViewParent CreateUi(Transform parent)
        {
            InventoryUi ui = (InventoryUi) base.CreateUi(parent);
            ui.SetCentralTextCallback(() => Name + " x" + Quantity());
            return ui;
        }

        public void AddOnUpdate(Action<Number> action)
        {
            _quantity.AddOnValueChange(action);
        }

        public float Decrement(float amount)
        {
            float previousQuantity = _quantity.CurrentValue();
            _quantity.SetCurrentValue(_quantity.CurrentValue() - amount);
            float consumption = previousQuantity - _quantity.CurrentValue();
            return consumption;
        }

        public void Increment(float amount)
        {
            _quantity.SetCurrentValue(_quantity.CurrentValue() + amount);
        }

        public float GetWeight(float quantity)
        {
            return quantity * Weight;
        }

        public void SetMax(float max)
        {
            _quantity.Max = max;
        }
    }
}