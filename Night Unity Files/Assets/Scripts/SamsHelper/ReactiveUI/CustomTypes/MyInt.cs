using System;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using Random = UnityEngine.Random;

namespace SamsHelper.ReactiveUI.CustomTypes
{
    public class MyInt : MyValue<int>
    {
        private int _min;
        private int _max;
        private Action _onMax;
        private Action _onMin;
        
        public MyInt(int initialValue, int min = 0, int max = int.MaxValue) : base(initialValue)
        {
            _min = min;
            _max = max;
            BroadcastChange();
        }

        public float AsPercent() => 100f / _max * _currentValue;

        public void OnMax(Action a)=> _onMax = a;
        public void OnMin(Action a) => _onMin = a;
        
        public int RandomInRange() => Random.Range(_min, _max);

        public int Max
        {
            get { return _max; }
            set
            {
                _max = value;
                if (_currentValue > _max)
                {
                    Val = _max;
                }
            }
        }

        public int Min
        {
            get { return _min; }
            set
            {
                _min = value;
                if (_currentValue < _min)
                {
                    Val = _min;
                }
            }
        }

        public int Val
        {
            get { return _currentValue; }
            set
            {
                if (value > _max)
                {
                    _currentValue = _max;
                }
                else if (value < _min)
                {
                    _currentValue = _min;
                }
                else
                {
                    _currentValue = value;
                }
                BroadcastChange();
            }
        }

        public bool ReachedMin()
        {
            bool reached = _currentValue <= _min;
            _onMin?.Invoke();
            return reached;
        }

        public bool ReachedMax()
        {
            bool reached = _currentValue >= _max;
            _onMax?.Invoke();
            return reached;
        }

        //OPERATORS
        public static int operator +(MyInt a, MyInt b)
        {
            return a._currentValue + b._currentValue;
        }

        public static int operator +(MyInt a, int b)
        {
            return a._currentValue + b;
        }

        public static int operator /(MyInt a, int b)
        {
            return a._currentValue / b;
        }

        public static bool operator <(MyInt a, MyInt b)
        {
            return a._currentValue < b._currentValue;
        }

        public static bool operator <(MyInt a, int b)
        {
            return a._currentValue < b;
        }

        public static bool operator <(int a, MyInt b)
        {
            return a < b._currentValue;
        }

        public static bool operator >(MyInt a, MyInt b)
        {
            return a._currentValue > b._currentValue;
        }

        public static bool operator >(MyInt a, int b)
        {
            return a._currentValue > b;
        }

        public static bool operator >(int a, MyInt b)
        {
            return a > b._currentValue;
        }

        public static int operator *(MyInt a, int b)
        {
            return a._currentValue + b;
        }

        public static bool operator ==(MyInt a, int b)
        {
            return a._currentValue == b;
        }

        public static bool operator !=(MyInt a, int b)
        {
            return a._currentValue != b;
        }

        public static bool operator ==(MyInt a, MyInt b)
        {
            return a._currentValue == b._currentValue;
        }

        public static bool operator !=(MyInt a, MyInt b)
        {
            return a._currentValue != b._currentValue;
        }

        private bool Equals(MyInt other)
        {
            return _currentValue.Equals(other._currentValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((MyInt) obj);
        }

        public override int GetHashCode()
        {
            return _currentValue.GetHashCode();
        }
    }
}