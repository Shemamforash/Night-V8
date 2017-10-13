using Random = UnityEngine.Random;

namespace SamsHelper.ReactiveUI.CustomTypes
{
    public class MyFloat : MyValue<float>
    {
        private float _min;
        private float _max;

        public MyFloat() : this(0, 0, float.MaxValue)
        {
        }

        public MyFloat(float initialValue) : base(initialValue)
        {
            BroadcastChange();
        }

        public MyFloat(float initialValue, float min, float max) : this(initialValue)
        {
            _min = min;
            _max = max;
        }

        public float Max
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

        public float Min
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

        public override void SetCurrentValue(float value)
        {
            if (value > _max)
            {
                CurrentValue = _max;
            }
            else if (value < _min)
            {
                CurrentValue = _min;
            }
            else
            {
                CurrentValue = value;
            }
            BroadcastChange();
        }

        public float RandomInRange()
        {
            return Random.Range(_min, _max);
        }

        public bool ReachedMin()
        {
            if (CurrentValue == _min)
            {
                return true;
            }
            return false;
        }

        public bool ReachedMax()
        {
            if (CurrentValue == _max)
            {
                return true;
            }
            return false;
        }

        //OPERATORS
        public static float operator +(MyFloat a, MyFloat b)
        {
            return a.CurrentValue + b.CurrentValue;
        }

        public static float operator +(MyFloat a, float b)
        {
            return a.CurrentValue + b;
        }

        public static float operator /(MyFloat a, float b)
        {
            return a.CurrentValue / b;
        }

        public static bool operator <(MyFloat a, MyFloat b)
        {
            return a.CurrentValue < b.CurrentValue;
        }

        public static bool operator <(MyFloat a, float b)
        {
            return a.CurrentValue < b;
        }

        public static bool operator <(float a, MyFloat b)
        {
            return a < b.CurrentValue;
        }

        public static bool operator >(MyFloat a, MyFloat b)
        {
            return a.CurrentValue > b.CurrentValue;
        }

        public static bool operator >(MyFloat a, float b)
        {
            return a.CurrentValue > b;
        }

        public static bool operator >(float a, MyFloat b)
        {
            return a > b.CurrentValue;
        }

        public static float operator *(MyFloat a, float b)
        {
            return a.CurrentValue + b;
        }

        public static bool operator ==(MyFloat a, float b)
        {
            return a.CurrentValue == b;
        }

        public static bool operator !=(MyFloat a, float b)
        {
            return a.CurrentValue != b;
        }

        public static bool operator ==(MyFloat a, MyFloat b)
        {
            return a.CurrentValue == b.CurrentValue;
        }

        public static bool operator !=(MyFloat a, MyFloat b)
        {
            return a.CurrentValue != b.CurrentValue;
        }

        protected bool Equals(MyFloat other)
        {
            return CurrentValue.Equals(other.CurrentValue);
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
            return CurrentValue.GetHashCode();
        }
    }
}