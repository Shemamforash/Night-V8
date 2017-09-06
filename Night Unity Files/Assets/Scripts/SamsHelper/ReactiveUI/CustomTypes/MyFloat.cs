using System;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using Random = UnityEngine.Random;

namespace SamsHelper.ReactiveUI.CustomTypes
{
    public class MyFloat : MyValue<float>
    {
        private event Action<float> OnValueChange;
        
        private readonly bool _valueCapped;
        private bool _treatAsInt;
        private float _min;
        private float _max;
        private float _currentValue;
        public string Name = "";

        public MyFloat(string name) : this(0, 0, float.MaxValue)
        {
            Name = name;
        }

        public MyFloat() : this(0, 0, float.MaxValue)
        {
        }

        public MyFloat(float initialValue)
        {
            _currentValue = initialValue;
            if (OnValueChange != null) OnValueChange(_currentValue);
//            UpdateLinkedTexts(_currentValue);
        }

        public MyFloat(float initialValue, float min, float max) : this(initialValue)
        {
            _min = min;
            _max = max;
            _valueCapped = true;
        }

        public float Max
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

        public float Min
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


        public void TreatAsInt()
        {
            _treatAsInt = true;
        }

        public void AddOnValueChange(Action<float> action)
        {
            action(_currentValue);
            OnValueChange += action;
        }
        
        public float Val
        {
            get
            {
                if (_treatAsInt)
                {
                    return (int) _currentValue;
                }
                return _currentValue;
            }
            set
            {
                if (_valueCapped)
                {
                    bool exceeded = false;
                    if (value > _max)
                    {
                        _currentValue = _max;
                        exceeded = true;
                    }
                    else if (value < _min)
                    {
                        _currentValue = _min;
                        exceeded = true;
                    }
                    if (exceeded)
                    {
#if UNITY_EDITOR
//                        throw new Exceptions.CappedValueExceededBoundsException();
#endif
                    }
                    _currentValue = value;
                }
                else
                {
                    _currentValue = value;
                }
                if (OnValueChange != null) OnValueChange(_currentValue);
//                UpdateLinkedTexts(_currentValue);
            }
        }

        public float RandomInRange()
        {
            return Random.Range(_min, _max);
        }

        public bool ReachedMin()
        {
            if (_currentValue == _min)
            {
                return true;
            }
            return false;
        }

        public bool ReachedMax()
        {
            if (_currentValue == _max)
            {
                return true;
            }
            return false;
        }
        
        //OPERATORS
        public static float operator +(MyFloat a, MyFloat b)
        {
            return a._currentValue + b._currentValue;
        }

        public static float operator +(MyFloat a, float b)
        {
            return a._currentValue + b;
        }

        public static float operator /(MyFloat a, float b)
        {
            return a._currentValue / b;
        }

        public static bool operator <(MyFloat a, MyFloat b)
        {
            return a._currentValue < b._currentValue;
        }

        public static bool operator <(MyFloat a, float b)
        {
            return a._currentValue < b;
        }

        public static bool operator <(float a, MyFloat b)
        {
            return a < b._currentValue;
        }

        public static bool operator >(MyFloat a, MyFloat b)
        {
            return a._currentValue > b._currentValue;
        }

        public static bool operator >(MyFloat a, float b)
        {
            return a._currentValue > b;
        }

        public static bool operator >(float a, MyFloat b)
        {
            return a > b._currentValue;
        }

        public static float operator *(MyFloat a, float b)
        {
            return a._currentValue + b;
        }

        public static bool operator ==(MyFloat a, float b)
        {
            return a._currentValue == b;
        }

        public static bool operator !=(MyFloat a, float b)
        {
            return a._currentValue != b;
        }

        public static bool operator ==(MyFloat a, MyFloat b)
        {
            return a._currentValue == b._currentValue;
        }

        public static bool operator !=(MyFloat a, MyFloat b)
        {
            return a._currentValue != b._currentValue;
        }

        protected bool Equals(MyFloat other)
        {
            return _currentValue.Equals(other._currentValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MyFloat) obj);
        }

        public override int GetHashCode()
        {
            return _currentValue.GetHashCode();
        }
    }
}