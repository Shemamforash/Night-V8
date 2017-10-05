using System;
using SamsHelper.ReactiveUI.CustomTypes;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public class IntAttribute : MyInt, IAttribute
    {
        public readonly AttributeType AttributeType;
        private float _summativeModifier, _multiplicativeModifier;
        private float _calculatedValue;

        public IntAttribute(AttributeType attributeType, int value, int min = 0, int max = 0) : base(value, min, max)
        {
            AttributeType = attributeType;
        }

        public float CalculatedValue()
        {
            return (int) Math.Round(_calculatedValue);
        }

        public void AddModifier(float modifier, bool summative = false)
        {
            if (summative) _summativeModifier += modifier;
            else _multiplicativeModifier += modifier;
            Recalculate();
        }

        public void Recalculate()
        {
            _calculatedValue = (Val + _summativeModifier) * _multiplicativeModifier;
        }

        public void RemoveModifier(float modifier, bool summative = false)
        {
            if (summative) _summativeModifier -= modifier;
            else _multiplicativeModifier -= modifier;
            Recalculate();
        }
    }
}