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

        public MyInt() : this(0, 0, int.MaxValue)
        {
        }

        public MyInt(int initialValue) : base(initialValue)
        {
            BroadcastChange();
        }

        public MyInt(int initialValue, int min, int max) : this(initialValue)
        {
            _min = min;
            _max = max;
        }

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

        public int RandomInRange()
        {
            return Random.Range(_min, _max);
        }

        public bool ReachedMin()
        {
            if (_currentValue <= _min)
            {
                return true;
            }
            return false;
        }

        public bool ReachedMax()
        {
            if (_currentValue >= _max)
            {
                return true;
            }
            return false;
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

        protected bool Equals(MyInt other)
        {
            return _currentValue.Equals(other._currentValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MyInt) obj);
        }

        public override int GetHashCode()
        {
            return _currentValue.GetHashCode();
        }
    }
}