using SamsHelper.ReactiveUI;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public class CharacterAttribute : MyValue
    {
        public readonly AttributeType AttributeType;
        private float _summativeModifier, _multiplicativeModifier = 1;
        private float _calculatedValue;

        public CharacterAttribute(AttributeType attributeType, float value, float min = 0, float max = float.MaxValue) : base(value, min, max)
        {
            AttributeType = attributeType;
        }

        public float GetCalculatedValue()
        {
            Recalculate();
            return _calculatedValue;
        }

        public void AddModifier(float modifier, bool summative = false)
        {
            if (summative) _summativeModifier += modifier;
            else _multiplicativeModifier += modifier;
            Recalculate();
        }

        public void Recalculate()
        {
            _calculatedValue = (GetCurrentValue() + _summativeModifier) * _multiplicativeModifier;
        }

        public void RemoveModifier(float modifier, bool summative = false)
        {
            if (summative) _summativeModifier -= modifier;
            else _multiplicativeModifier -= modifier;
            Recalculate();
        }
    }
}