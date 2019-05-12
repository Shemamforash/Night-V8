using UnityEngine;

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
				_max         = value;
				CurrentValue = _currentValue;
			}
		}

		public float Min
		{
			get => _min;
			set
			{
				_min         = value;
				CurrentValue = _currentValue;
			}
		}

		public virtual float CurrentValue
		{
			get => _currentValue;
			set => _currentValue = Mathf.Clamp(value, _min, _max);
		}

		public float Normalised => _currentValue / _max;
		public bool  ReachedMin => _currentValue <= _min;
		public bool  ReachedMax => _currentValue >= _max;

		public virtual void Increment(float amount = 1) => CurrentValue = _currentValue + amount;


		//OPERATORS
		public static float operator +(Number a, Number b) => a._currentValue + b._currentValue;
		public static float operator +(Number a, float  b) => a._currentValue + b;
		public static float operator /(Number a, float  b) => a._currentValue / b;
		public static bool operator <(Number  a, Number b) => a._currentValue < b._currentValue;
		public static bool operator <(Number  a, float  b) => a._currentValue < b;
		public static bool operator <(float   a, Number b) => a               < b._currentValue;
		public static bool operator >(Number  a, Number b) => a._currentValue > b._currentValue;
		public static bool operator >(Number  a, float  b) => a._currentValue > b;
		public static bool operator >(float   a, Number b) => a               > b._currentValue;
		public static float operator *(Number a, float  b) => a._currentValue + b;
	}
}