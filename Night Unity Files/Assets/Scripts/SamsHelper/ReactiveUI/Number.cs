using Random = UnityEngine.Random;

namespace SamsHelper.ReactiveUI
{
	public class Number
	{
		private float _currentValue;
		private float _max;
		private float _min;

		public Number(float initialValue = 0, float min = 0, float max = 1000000)
		{
			_currentValue = initialValue;
			_min          = min;
			_max          = max;
		}

		public float Max
		{
			get => _max;
			set
			{
				_max          = value;
				_currentValue = _currentValue > _max ? _max : _currentValue;
			}
		}

		public float Min
		{
			get => _min;
			set
			{
				_min          = value;
				_currentValue = _currentValue < _min ? _min : _currentValue;
			}
		}

		public virtual float CurrentValue
		{
			get => _currentValue;
			set
			{
				if (value >= _max)
				{
					_currentValue = _max;
				}
				else if (value <= _min)
				{
					_currentValue = _min;
				}
				else
				{
					_currentValue = value;
				}
			}
		}

		public float Normalised => _currentValue / _max;

		public float RandomInRange()
		{
			return Random.Range(_min, _max);
		}

		public bool ReachedMin => _currentValue <= _min;

		public bool ReachedMax => _currentValue >= _max;

		public virtual void Increment(float amount = 1)
		{
			_currentValue += amount;
		}

		public virtual void Decrement(float amount = 1)
		{
			_currentValue -= amount;
		}


		//OPERATORS
		public static float operator +(Number a, Number b)
		{
			return a._currentValue + b._currentValue;
		}

		public static float operator +(Number a, float b)
		{
			return a._currentValue + b;
		}

		public static float operator /(Number a, float b)
		{
			return a._currentValue / b;
		}

		public static bool operator <(Number a, Number b)
		{
			return a._currentValue < b._currentValue;
		}

		public static bool operator <(Number a, float b)
		{
			return a._currentValue < b;
		}

		public static bool operator <(float a, Number b)
		{
			return a < b._currentValue;
		}

		public static bool operator >(Number a, Number b)
		{
			return a._currentValue > b._currentValue;
		}

		public static bool operator >(Number a, float b)
		{
			return a._currentValue > b;
		}

		public static bool operator >(float a, Number b)
		{
			return a > b._currentValue;
		}

		public static float operator *(Number a, float b)
		{
			return a._currentValue + b;
		}
	}
}