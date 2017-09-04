using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.CustomTypes;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class InventoryResource : BasicInventoryContents
    {
        private readonly MyFloat _quantity = new MyFloat(0, 0, float.MaxValue);

        public InventoryResource(string name, float weight) : base(name, weight)
        {
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

        public override float Quantity()
        {
            return _quantity.Val;
        }

        public float GetWeight(float quantity)
        {
            return quantity * Weight();
        }

        public float GetTotalWeight()
        {
            return Weight() * _quantity.Val;
        }
    }
}