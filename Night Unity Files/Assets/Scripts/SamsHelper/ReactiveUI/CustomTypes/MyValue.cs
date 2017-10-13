using System;

namespace SamsHelper.ReactiveUI.CustomTypes
{
    public abstract class MyValue<T>
    {
        protected event Action<MyValue<T>> OnValueChange;
        protected T CurrentValue;

        protected MyValue(T initialValue)
        {
            CurrentValue = initialValue;
        }

        public void AddOnValueChange(Action<MyValue<T>> a)
        {
            a(this);
            OnValueChange += a;
        }

        protected void BroadcastChange()
        {
            OnValueChange?.Invoke(this);
        }

        public T GetCurrentValue() => CurrentValue;
        public abstract void SetCurrentValue(T value);
    }
}