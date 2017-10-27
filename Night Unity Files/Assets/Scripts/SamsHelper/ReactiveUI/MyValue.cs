using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        private readonly LinkedList<Tuple<float, string>> _thresholdList = new LinkedList<Tuple<float, string>>();

        public MyValue(float initialValue = 0, float min = 0, float max = float.MaxValue)
        {
            _currentValue = initialValue;
            _min = min;
            _max = max;
            BroadcastChange();
        }

        public void AddThreshold(float thresholdValue, string thresholdName)
        {
            if (thresholdValue < _min || thresholdValue > _max)
            {
                throw new Exceptions.ThresholdValueNotReachableException(thresholdName, thresholdValue, _min, _max);
            }
            Tuple<float, string> newThreshold = Tuple.Create(thresholdValue, thresholdName);
            LinkedListNode<Tuple<float, string>> current = _thresholdList.First;
            if (current == null)
            {
                _thresholdList.AddFirst(newThreshold);
            }
            else
            {
                while (current != null)
                {
                    if (thresholdValue < current.Value.Item1)
                    {
                        _thresholdList.AddAfter(current, newThreshold);
                        break;
                    }
                    current = current.Next;
                }
                _thresholdList.AddAfter(_thresholdList.Last, newThreshold);
            }
        }

        public string GetThresholdName()
        {
            return (from threshold in _thresholdList where _currentValue <= threshold.Item1 select threshold.Item2).FirstOrDefault();
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

        public void Increment(float amount)
        {
            SetCurrentValue(_currentValue + amount);
        }

        public void Decrement(float amount)
        {
            SetCurrentValue(_currentValue - amount);
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