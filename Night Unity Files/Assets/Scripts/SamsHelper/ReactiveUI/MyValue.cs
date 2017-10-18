using System;

namespace SamsHelper.ReactiveUI
{
    public class MyValue
    {
        protected event Action<MyValue> OnValueChange;
        private float _currentValue;
        private float _min;
        private float _max;
        private Action _onMax;
        private Action _onMin;

        public MyValue(float initialValue = 0, float min = 0, float max = float.MaxValue)
        {
            _currentValue = initialValue;
            _min = min;
            _max = max;
            BroadcastChange();
        }

        public void AddOnValueChange(Action<MyValue> a)
        {
            a(this);
            OnValueChange += a;
        }

        private void BroadcastChange()
        {
            OnValueChange?.Invoke(this);
        }

        public float GetCurrentValue() => _currentValue;
        public float AsPercent() => 100f / _max * _currentValue;
        public void OnMax(Action a) => _onMax = a;
        public void OnMin(Action a) => _onMin = a;
        public float RandomInRange() => UnityEngine.Random.Range(_min, _max);
        public bool ReachedMin() => _currentValue <= _min;
        public bool ReachedMax() => _currentValue >= _max;

        public float Max
        {
            get { return _max; }
            set
            {
                _max = value;
                if (_currentValue > _max)
                {
                    SetCurrentValue(_max);
                }
            }
        }

        public float Min
        {
            get { return _min; }
            set
            {
                _min = value;
                if (_currentValue < _min)
                {
                    SetCurrentValue(_min);
                }
            }
        }

        public void SetCurrentValue(float value)
        {
            if (value > _max)
            {
                _currentValue = _max;
                _onMax?.Invoke();
            }
            else if (value < _min)
            {
                _currentValue = _min;
                _onMin?.Invoke();
            }
            else
            {
                _currentValue = value;
            }
            BroadcastChange();
        }

        //OPERATORS
        public static float operator +(MyValue a, MyValue b)
        {
            return a._currentValue + b._currentValue;
        }

        public static float operator +(MyValue a, float b)
        {
            return a._currentValue + b;
        }

        public static float operator /(MyValue a, float b)
        {
            return a._currentValue / b;
        }

        public static bool operator <(MyValue a, MyValue b)
        {
            return a._currentValue < b._currentValue;
        }

        public static bool operator <(MyValue a, float b)
        {
            return a._currentValue < b;
        }

        public static bool operator <(float a, MyValue b)
        {
            return a < b._currentValue;
        }

        public static bool operator >(MyValue a, MyValue b)
        {
            return a._currentValue > b._currentValue;
        }

        public static bool operator >(MyValue a, float b)
        {
            return a._currentValue > b;
        }

        public static bool operator >(float a, MyValue b)
        {
            return a > b._currentValue;
        }

        public static float operator *(MyValue a, float b)
        {
            return a._currentValue + b;
        }

        public override int GetHashCode()
        {
            return _currentValue.GetHashCode();
        }
    }
}