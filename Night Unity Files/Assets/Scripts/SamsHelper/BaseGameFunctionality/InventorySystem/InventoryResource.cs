using System;
using SamsHelper.ReactiveUI.CustomTypes;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class InventoryResource : BasicInventoryItem
    {
        private readonly MyInt _quantity = new MyInt(0);

        public InventoryResource(string name, float weight) : base(name, GameObjectType.Resource, weight)
        {
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