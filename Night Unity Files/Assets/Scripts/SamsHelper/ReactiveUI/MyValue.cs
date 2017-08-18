using System.Collections.Generic;

namespace SamsHelper.ReactiveUI
{
    public abstract class MyValue<T>
    {
        private readonly List<ReactiveText<T>> _linkedTexts = new List<ReactiveText<T>>();

        public void AddLinkedText(ReactiveText<T> linkedText)
        {
            _linkedTexts.Add(linkedText);
        }
        
        public void UpdateLinkedTexts(T val)
        {
            _linkedTexts.ForEach(t => t.Text(val));
        }
    }
}