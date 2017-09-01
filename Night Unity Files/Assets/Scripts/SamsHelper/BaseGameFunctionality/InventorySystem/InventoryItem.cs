using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.CustomTypes;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class InventoryItem
    {
        private MyFloat _quantity;
        public string Name;

        private float _weight;

        //If stackable, quantity will increase beyond 1, otherwise each duplicate item is carried seperately in the inventory
        //If marked as destroyonempty, the inventory item will be deleted when its quantity reaches 0 (useful for resources).
        private bool _stackable, _destroyOnEmpty = true;

        public InventoryItem(string name, float weight)
        {
            Name = name;
            _weight = weight;
            _quantity = new MyFloat(0, 0, float.MaxValue);
            Decrement(0);
        }

        public InventoryItem Clone()
        {
            InventoryItem clone = new InventoryItem(Name, _weight)
            {
                _destroyOnEmpty = _destroyOnEmpty,
                _stackable = _stackable,
                _quantity = {Val = 1}
            };
            return clone;
        }

        public void DontDestroyOnEmpty()
        {
            _destroyOnEmpty = false;
        }

        public void AllowStacking()
        {
            _stackable = true;
        }

        public bool Stackable()
        {
            return _stackable;
        }

        public bool DestroyOnEmpty()
        {
            return _destroyOnEmpty;
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
            float previousQuantity = _quantity.Val;
            _quantity.Val = _quantity.Val - amount;
            float consumption = previousQuantity - _quantity.Val;
            return consumption;
        }

        public void Increment(float amount)
        {
            _quantity.Val += amount;
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