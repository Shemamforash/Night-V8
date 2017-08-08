using System.Collections.Generic;
using UnityEngine.UI;

namespace Game.Misc
{
    public class MyString
    {
        private List<Text> _associatedTexts;
        private string _text;

        public MyString(List<Text> associatedTexts)
        {
            _associatedTexts = associatedTexts;
            foreach (Text t in associatedTexts)
            {
                t.text = _text;
            }
        }

        public MyString(Text associatedText) : this(new List<Text> {associatedText})
        {
        }
    }
}