using System;
using UnityEngine;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI
{
    public class ReactiveText<T>
    {
        private readonly Text _associatedText;
        private Func<T, string> _formattingFunction;

        public ReactiveText(Text associatedText)
        {
            _associatedText = associatedText;
        }

        public ReactiveText(Text associatedText, Func<T, string> formattingFunction)
        {
            _associatedText = associatedText;
            _formattingFunction = formattingFunction;
        }

        public void SetFormattingFunction(Func<T, string> formattingFunction)
        {
            _formattingFunction = formattingFunction;
        }

        public void Text(T value)
        {
            if (_formattingFunction == null)
            {
                _associatedText.text = value.ToString();
            }
            else
            {
                _associatedText.text = _formattingFunction(value);
            }
        }

        public void Text(string text)
        {
            _associatedText.text = text;
        }
    }
}