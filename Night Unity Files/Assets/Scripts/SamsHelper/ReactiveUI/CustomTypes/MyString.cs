﻿namespace SamsHelper.ReactiveUI.CustomTypes
{
    public class MyString : MyValue<string>
    {
        private string _text;
        
        public MyString()
        {
            _text = "";
        }
        
        public MyString(string initialText)
        {
            _text = initialText;
            UpdateLinkedTexts(_text);
        }

        public string Text
        {
            set
            {
                _text = value;
                UpdateLinkedTexts(_text);
            }
            get { return _text; }
        }
    }
}