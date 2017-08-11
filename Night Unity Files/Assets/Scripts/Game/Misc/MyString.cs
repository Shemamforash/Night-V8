using System.Collections.Generic;
using UnityEngine.UI;

namespace Game.Misc
{
    public class MyString
    {
        private List<Text> _associatedTexts;
        private string _text;

        public MyString(string initialText, List<Text> associatedTexts)
        {
            _text = initialText;
            _associatedTexts = associatedTexts;
            UpdateAssociatedText();
        }

        public MyString(string initialText, Text associatedText) : this(initialText, new List<Text> {associatedText})
        {
        }

        private void UpdateAssociatedText()
        {
            foreach (Text t in _associatedTexts)
            {
                t.text = _text;
            }
        }

        public void SetText(string text)
        {
            _text = text;
            UpdateAssociatedText();
        }
    }
}