using UnityEngine;

namespace SamsHelper.ReactiveUI.CustomTypes
{
    public class MyFloat : MyValue<float>
    {
        protected bool Equals(MyFloat other)
        {
            return _currentValue.Equals(other._currentValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MyFloat) obj);
        }

        public override int GetHashCode()
        {
            return _currentValue.GetHashCode();
        }

        private readonly bool _valueCapped;
        private bool _treatAsInt;
        private float _min;
        private float _max;
        private float _currentValue;

        public MyFloat()
        {
            _currentValue = 0;
        }
        
        public MyFloat(float initialValue)
        {
            _currentValue = initialValue;
            UpdateLinkedTexts(_currentValue);
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
                    else
                    {
                        _currentValue = value;
                    }
                }
                else
                {
                    _currentValue = value;
                }
                UpdateLinkedTexts(_currentValue);
            }
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

        public float RandomInRange()
        {
            return Random.Range(_min, _max);
        }
    }
}