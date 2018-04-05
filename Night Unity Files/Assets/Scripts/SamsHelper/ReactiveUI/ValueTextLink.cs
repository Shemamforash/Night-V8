using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace SamsHelper.ReactiveUI
{
    public class ValueTextLink<T>
    {
        private readonly List<Tuple<TextMeshProUGUI, Func<T, string>>> _textAssociations = new List<Tuple<TextMeshProUGUI, Func<T, string>>>();
        private T _value;

        public void AddTextObject(TextMeshProUGUI textObject, Func<T, string> formattingFunction = null)
        {
            if (textObject == null) return;
            Tuple<TextMeshProUGUI, Func<T, string>> existingTuple = _textAssociations.FirstOrDefault(tup => tup.Item1 == textObject);
            Tuple<TextMeshProUGUI, Func<T, string>> newTuple = Tuple.Create(textObject, formattingFunction);
            if (existingTuple != null)
            {
                _textAssociations[_textAssociations.IndexOf(existingTuple)] = newTuple;
            }
            else
            {
                _textAssociations.Add(newTuple);
            }
            if(_value != null) UpdateText();
        }

        private void SetTextObjectText(Tuple<TextMeshProUGUI, Func<T, string>> tup, string defaultValue)
        {
            TextMeshProUGUI textObject = tup.Item1;
            textObject.text = tup.Item2 != null ? tup.Item2(_value) : defaultValue;
        }

        private void SetValue(T value)
        {
            Number number = value as Number;
            number?.AddOnValueChange(val => { _textAssociations.ForEach(tup => { SetTextObjectText(tup, val.CurrentValue().ToString()); }); });
        }

        public void SetEnabled(bool enable)
        {
            _textAssociations.ForEach(tup => { tup.Item1.gameObject.SetActive(enable); });
        }

        private void UpdateText()
        {
            _textAssociations.ForEach(tup => SetTextObjectText(tup, _value.ToString()));
        }
        
        public void Value(T value)
        {
            if (_value == null)
            {
                SetValue(value);
            }
            _value = value;
            UpdateText();
        }
    }
}