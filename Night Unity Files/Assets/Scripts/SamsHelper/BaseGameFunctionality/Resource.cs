using SamsHelper.ReactiveUI;

namespace Game.World
{
	public class Resource {
		private MyFloat _quantity;
		private string _name;
		
		public Resource(string name, float min, float max)
		{
			_name = name;
			_quantity = new MyFloat(0, min, max);
			Decrement(0);
		}

		public Resource(string name) : this(name, 0, float.MaxValue)
		{
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
			float previousQuantity = _quantity.Val;
			_quantity.Val -= amount;
			float consumption = previousQuantity - _quantity.Val;
			return consumption;
		}

		public void Increment(float amount){
			_quantity.Val += amount;
		}

		public float Quantity() {
			return _quantity.Val;
		}
	}
}
