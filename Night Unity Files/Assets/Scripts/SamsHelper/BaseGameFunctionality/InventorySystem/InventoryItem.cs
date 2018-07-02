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
        public readonly ResourceTemplate Template;
        private bool _isResource;

        public InventoryItem(ResourceTemplate template, GameObjectType type, Inventory parentInventory = null) : base(template.Name, type, parentInventory)
        {
            Template = template;
        }

        protected InventoryItem(string name, GameObjectType type, Inventory parentInventory = null) : base(name, type, parentInventory)
        {
            _isResource = false;
            _quantity.Increment();
        }

        public int Quantity()
        {
            return Mathf.FloorToInt(_quantity.CurrentValue());
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