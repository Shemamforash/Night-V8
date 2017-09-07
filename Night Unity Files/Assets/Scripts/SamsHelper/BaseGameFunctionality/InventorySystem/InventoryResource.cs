using System;
using SamsHelper.ReactiveUI.CustomTypes;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class InventoryResource : BasicInventoryContents
    {
        private readonly MyInt _quantity = new MyInt(0, 0, int.MaxValue);

        public InventoryResource(string name, float weight) : base(name, weight)
        {
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
            return quantity * Weight();
        }

        public float GetTotalWeight()
        {
            return Weight() * _quantity.Val;
        }
    }
}