﻿using System;
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

        public float AsPercent() => 100f / _max * CurrentValue;

        public void OnMax(Action a)=> _onMax = a;
        public void OnMin(Action a) => _onMin = a;
        
        public int RandomInRange() => Random.Range(_min, _max);

        public int Max
        {
            get { return _max; }
            set
            {
                _max = value;
                if (CurrentValue > _max)
                {
                    SetCurrentValue(_max);
                }
            }
        }

        public int Min
        {
            get { return _min; }
            set
            {
                _min = value;
                if (CurrentValue < _min)
                {
                    SetCurrentValue(_min);
                }
            }
        }

        public override void SetCurrentValue(int value)
        {
            if (value > _max)
            {
                CurrentValue = _max;
                _onMax?.Invoke();
            }
            else if (value < _min)
            {
                CurrentValue = _min;
                _onMin?.Invoke();
            }
            else
            {
                CurrentValue = value;
            }
            BroadcastChange();
        }

        public bool ReachedMin()
        {
            bool reached = CurrentValue <= _min;
            return reached;
        }

        public bool ReachedMax()
        {
            bool reached = CurrentValue >= _max;
            return reached;
        }

        //OPERATORS
        public static int operator +(MyInt a, MyInt b)
        {
            return a.CurrentValue + b.CurrentValue;
        }

        public static int operator +(MyInt a, int b)
        {
            return a.CurrentValue + b;
        }

        public static int operator /(MyInt a, int b)
        {
            return a.CurrentValue / b;
        }

        public static bool operator <(MyInt a, MyInt b)
        {
            return a.CurrentValue < b.CurrentValue;
        }

        public static bool operator <(MyInt a, int b)
        {
            return a.CurrentValue < b;
        }

        public static bool operator <(int a, MyInt b)
        {
            return a < b.CurrentValue;
        }

        public static bool operator >(MyInt a, MyInt b)
        {
            return a.CurrentValue > b.CurrentValue;
        }

        public static bool operator >(MyInt a, int b)
        {
            return a.CurrentValue > b;
        }

        public static bool operator >(int a, MyInt b)
        {
            return a > b.CurrentValue;
        }

        public static int operator *(MyInt a, int b)
        {
            return a.CurrentValue + b;
        }

        public static bool operator ==(MyInt a, int b)
        {
            return a.CurrentValue == b;
        }

        public static bool operator !=(MyInt a, int b)
        {
            return a.CurrentValue != b;
        }

        public static bool operator ==(MyInt a, MyInt b)
        {
            return a.CurrentValue == b.CurrentValue;
        }

        public static bool operator !=(MyInt a, MyInt b)
        {
            return a.CurrentValue != b.CurrentValue;
        }

        private bool Equals(MyInt other)
        {
            return CurrentValue.Equals(other.CurrentValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((MyInt) obj);
        }

        public override int GetHashCode()
        {
            return CurrentValue.GetHashCode();
        }
    }
}