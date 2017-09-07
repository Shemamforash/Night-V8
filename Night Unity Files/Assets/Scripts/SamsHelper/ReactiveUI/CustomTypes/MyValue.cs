using System;

namespace SamsHelper.ReactiveUI.CustomTypes
{
    public abstract class MyValue<T>
    {
        protected event Action<T> OnValueChange;
        protected T _currentValue;

        public MyValue(T initialValue)
        {
            _currentValue = initialValue;
        }

        public void AddOnValueChange(Action<T> a)
        {
            a(_currentValue);
            OnValueChange += a;
        }

        public void BroadcastChange()
        {
            if (OnValueChange != null)
            {
                OnValueChange(_currentValue);
            }
        }
    }
}