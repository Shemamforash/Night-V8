using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Misc
{
    public partial class MyFloat
    {
        private readonly float _min;
        private readonly float _max;
        private float _currentValue;
        private readonly bool _capped, _throwException;
        private List<TextAssociation> _associatedTexts = new List<TextAssociation>();

        public MyFloat(float value)
        {
            Value = value;
            _capped = false;
        }

        public MyFloat(float value, float min, float max)
        {
            Value = value;
            _min = min;
            _max = max;
            _capped = true;
        }

        public MyFloat(float value, float min, float max, bool throwException) : this(value, min, max)
        {
            _throwException = throwException;
        }

        public MyFloat(float value, List<TextAssociation> associatedTexts)
        {
            _associatedTexts = associatedTexts;
            Value = value;
        }

        public MyFloat(float value, List<TextAssociation> associatedTexts, float min, float max) : this(value,
            associatedTexts)
        {
            _capped = true;
            _min = min;
            _max = max;
        }

        public MyFloat(float value, TextAssociation associatedText) : this(value,
            new List<TextAssociation> {associatedText})
        {
        }

        public MyFloat(float value, TextAssociation associatedText, float min, float max) : this(value,
            new List<TextAssociation> {associatedText}, min, max)
        {
        }

        public static float operator +(MyFloat a, MyFloat b)
        {
            return a.Value + b.Value;
        }
        
        public static float operator +(MyFloat a, float b)
        {
            return a.Value + b;
        }

        public static float operator /(MyFloat a, float b)
        {
            return a.Value / b;
        }

        public static bool operator <(MyFloat a, MyFloat b)
        {
            return a.Value < b.Value;
        }

        public static bool operator <(MyFloat a, float b)
        {
            return a.Value < b;
        }
        
        public static bool operator <(float a, MyFloat b)
        {
            return a < b.Value;
        }

        public static bool operator >(MyFloat a, MyFloat b)
        {
            return a.Value > b.Value;
        }

        public static bool operator >(MyFloat a, float b)
        {
            return a.Value > b;
        }
        
        public static bool operator >(float a, MyFloat b)
        {
            return a > b.Value;
        }

        public static float operator *(MyFloat a, float b)
        {
            return a.Value + b;
        }

        public static bool operator ==(MyFloat a, float b)
        {
            return a.Value == b;
        }

        public static bool operator !=(MyFloat a, float b)
        {
            return a.Value != b;
        }

        public float Max()
        {
            return _max;
        }

        public void AddAssociatedText(TextAssociation associatedText)
        {
            _associatedTexts.Add(associatedText);
            UpdateText();
        }

        public void UpdateText()
        {
            foreach (TextAssociation t in _associatedTexts)
            {
                t.UpdateText(_currentValue);
            }
        }

        public float Value
        {
            get { return _currentValue; }
            set
            {
                if (_capped)
                {
                    if (value > _max)
                    {
                        if (_throwException)
                        {
                            throw new Exceptions.CappedValueExceededBoundsException();
                        }
                        _currentValue = _max;
                    }
                    else if (value < _min)
                    {
                        if (_throwException)
                        {
                            throw new Exceptions.CappedValueExceededBoundsException();
                        }
                        _currentValue = _min;
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
                UpdateText();
            }
        }
    }
}