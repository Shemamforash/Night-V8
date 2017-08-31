using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.CustomTypes;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class InventoryItem
    {
        private MyFloat _quantity;
        public string Name;
        private float _weight;
        private bool _stackable = true;

        public InventoryItem(string name, float weight, float min, float max)
        {
            Name = name;
            _weight = weight;
            _quantity = new MyFloat(0, min, max);
            Decrement(0);
        }

        public InventoryItem(string name, float weight) : this(name, weight, 0, float.MaxValue)
        {
        }

        public void SetUnique()
        {
            _stackable = false;
        }

        public void SetMin(float min)
        {
            _quantity.Min = min;
        }

        public void SetMax(float max)
        {
            _quantity.Max = max;
        }

        public void AddLinkedText(ReactiveText<float> reactiveText)
        {
            _quantity.AddLinkedText(reactiveText);
            _quantity.UpdateLinkedTexts(_quantity.Val);
        }

        public float Decrement(float amount)
        {
            if (_stackable)
            {
                float previousQuantity = _quantity.Val;
                _quantity.Val -= amount;
                float consumption = previousQuantity - _quantity.Val;
                return consumption;
            }
            throw new Exceptions.InventoryItemNotStackableException(Name, -amount);
        }

        public void Increment(float amount)
        {
            if (_stackable)
            {
                _quantity.Val += amount;
            }
            else
            {
                throw new Exceptions.InventoryItemNotStackableException(Name, amount);
            }
        }

        public float Quantity()
        {
            return _quantity.Val;
        }

        public float GetWeight(int quantity)
        {
            return quantity * _weight;
        }

        public float GetTotalWeight()
        {
            return _weight * _quantity.Val;
        }
    }
}