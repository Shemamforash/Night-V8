using System;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

namespace SamsHelper.ReactiveUI
{
    public class Number
    {
        protected event Action<Number> OnValueChange;
        private float _currentValue;
        private float _max;
        private float _min;
        private Action _onMax;
        private Action _onMin;

        public Number(float initialValue = 0, float min = 0, float max = float.MaxValue)
        {
            _currentValue = initialValue;
            _min = min;
            _max = max;
            BroadcastChange();
        }

        public float Max
        {
            get { return _max; }
            set
            {
                _max = value;
                SetCurrentValue(_currentValue > _max ? _max : _currentValue);
            }
        }

        public float Min
        {
            get { return _min; }
            set
            {
                _min = value;
                SetCurrentValue(_currentValue < _min ? _min : _currentValue);
            }
        }

        public void AddOnValueChange(Action<Number> a)
        {
            a(this);
            OnValueChange += a;
        }

        public void UpdateValueChange()
        {
            OnValueChange?.Invoke(this);
        }
        
        public void ClearOnValueChange()
        {
            OnValueChange = null;
        }

        private void BroadcastChange()
        {
            OnValueChange?.Invoke(this);
        }

        public virtual float CurrentValue()
        {
            return _currentValue;
        }

        public float Normalised()
        {
            return _currentValue / _max;
        }

        public void OnMax(Action a)
        {
            _onMax = a;
        }

        public void OnMin(Action a)
        {
            _onMin = a;
        }

        public float RandomInRange()
        {
            return Random.Range(_min, _max);
        }

        public bool ReachedMin()
        {
            return _currentValue <= _min;
        }

        public bool ReachedMax()
        {
            return _currentValue >= _max;
        }

        public void Increment(float amount = 1)
        {
            SetCurrentValue(_currentValue + amount);
        }

        public void Decrement(float amount = 1)
        {
            SetCurrentValue(_currentValue - amount);
        }

        public void SetCurrentValue(float value)
        {
            if (value >= _max)
            {
                _currentValue = _max;
                _onMax?.Invoke();
            }
            else if (value <= _min)
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

        public override int GetHashCode()
        {
            return _currentValue.GetHashCode();
        }
    }
}