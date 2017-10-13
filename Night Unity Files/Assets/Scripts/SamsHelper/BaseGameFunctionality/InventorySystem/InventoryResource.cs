using System;
using SamsHelper.ReactiveUI.CustomTypes;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class InventoryResource : InventoryItem
    {
        public readonly MyInt Quantity = new MyInt(0);
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

        public override InventoryUi CreateUi(Transform parent)
        {
            InventoryUi ui = base.CreateUi(parent);
            ui.SetRightTextCallback(() => "x" + Quantity());
            return ui;
        }
        
        public void AddOnUpdate(Action<MyValue<int>> action)
        {
            Quantity.AddOnValueChange(action);
        }

        public int Decrement(int amount)
        {
            int previousQuantity = Quantity.GetCurrentValue();
            Quantity.SetCurrentValue(Quantity.GetCurrentValue() - amount);
            int consumption = previousQuantity - Quantity.GetCurrentValue();
            return consumption;
        }

        public void Increment(int amount)
        {
            Quantity.SetCurrentValue(Quantity.GetCurrentValue() + amount);
        }

        public float GetWeight(int quantity)
        {
            return quantity * Weight;
        }
    }
}