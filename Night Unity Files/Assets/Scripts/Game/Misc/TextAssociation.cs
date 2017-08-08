using System;
using UnityEngine.UI;

namespace Game.Misc
{
    public class TextAssociation
    {
        private readonly Text _associatedText;
        private readonly Func<float, string> _formattingFunction;

        public TextAssociation(Text associatedText)
        {
            _associatedText = associatedText;
        }

        public TextAssociation(Text associatedText, Func<float, string> formattingFunction)
        {
            _associatedText = associatedText;
            _formattingFunction = formattingFunction;
        }
        
        public void UpdateText(float roundedValue)
        {
            if (_formattingFunction == null)
            {
                _associatedText.text = roundedValue.ToString();
            }
            else
            {
                _associatedText.text = _formattingFunction(roundedValue);
            }
        }
    }
}