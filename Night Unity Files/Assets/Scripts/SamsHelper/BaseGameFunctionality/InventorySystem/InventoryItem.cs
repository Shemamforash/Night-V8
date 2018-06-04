using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class InventoryItem : MyGameObject
    {
        private readonly Number _quantity = new Number();
        private bool _stackable;
        
        public InventoryItem(string name, GameObjectType type, float weight, Inventory parentInventory = null) : base(name, type, weight, parentInventory)
        {
        }

        public int Quantity()
        {
            return Mathf.FloorToInt(_quantity.CurrentValue());
        }

        public float TotalWeight()
        {
            return Helper.Round(Weight * Quantity(), 1);
        }

        public void Increment(int amount)
        {
            _quantity.SetCurrentValue(_quantity.CurrentValue() + amount);
        }

        public void Decrement(int amount)
        {
            _quantity.SetCurrentValue(_quantity.CurrentValue() - amount);
        }

        public virtual bool IsStackable()
        {
            return _stackable;
        }

        public void SetStackable(bool stackable)
        {
            _stackable = stackable;
        }
    }
}