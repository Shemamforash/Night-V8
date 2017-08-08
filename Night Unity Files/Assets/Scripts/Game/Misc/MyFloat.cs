using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Misc
{
    public class MyFloat
    {
        private readonly float _min;
        private readonly float _max;
        private float _currentValue;
        private readonly bool _capped, _throwException;
        private Text _associatedText;

        private Func<float, string> _formattingFunction;

        //Defauly 0 decimal places
        private float _precisionDivisor = 1f;

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

        public MyFloat(float value, float min, float max, bool throwException)
        {
            Value = value;
            _min = min;
            _max = max;
            _throwException = throwException;
        }

        public MyFloat(float value, Text associatedText)
        {
            _associatedText = associatedText;
            Value = value;
        }

        public MyFloat(float value, Text associatedText, float min, float max)
        {
            _associatedText = associatedText;
            Value = value;
            _capped = true;
            _min = min;
            _max = max;
        }

        public static float operator +(MyFloat a, MyFloat b)
        {
            return a.Value + b.Value;
        }

        public static float operator /(MyFloat a, float b)
        {
            return a.Value / b;
        }

        public float Max()
        {
            return _max;
        }
        
        public void SetAssociatedText(Text associatedText)
        {
            _associatedText = associatedText;
        }

        public void SetFormattingFunction(Func<float, string> formattingFunction)
        {
            _formattingFunction = formattingFunction;
            UpdateText();
        }

        public void SetDecimalPlaces(int places)
        {
            _precisionDivisor = Mathf.Pow(10, places);
        }

        public void UpdateText()
        {
            if (_associatedText != null)
            {
                if (_formattingFunction != null)
                {
                    _associatedText.text = _formattingFunction(RoundValue());
                }
                else
                {
                    _associatedText.text = RoundValue().ToString();
                }
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

        private float RoundValue()
        {
            return Mathf.Round(_currentValue * _precisionDivisor) / _precisionDivisor;
        }
    }
}