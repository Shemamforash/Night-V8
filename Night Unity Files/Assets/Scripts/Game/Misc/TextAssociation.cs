using System;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Misc
{
    public class TextAssociation
    {
        private readonly Text _associatedText;
        private readonly Func<float, string> _formattingFunction;
        private bool _roundValue;
        private float _precision = 10f;
        
        public TextAssociation(Text associatedText)
        {
            _associatedText = associatedText;
        }

        public TextAssociation(Text associatedText, Func<float, string> formattingFunction, bool roundValue)
        {
            _associatedText = associatedText;
            _formattingFunction = formattingFunction;
            _roundValue = roundValue;
        }

        public void SetPrecision(int n)
        {
            _precision = (float)Math.Pow(10f, n);
        }

        public float Round(float value)
        {
            return Mathf.Round(value * _precision) / _precision;
        }
        
        public void UpdateText(float value)
        {
            if (_roundValue)
            {
                value = Round(value);
            }
            if (_formattingFunction == null)
            {
                _associatedText.text = value.ToString();
            }
            else
            {
                _associatedText.text = _formattingFunction(value);
            }
        }
    }
}