using SamsHelper.ReactiveUI.CustomTypes;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public class FloatAttribute : MyFloat, IAttribute
    {
        public readonly AttributeType AttributeType;
        private float _summativeModifier, _multiplicativeModifier;
        private float _calculatedValue;

        public FloatAttribute(AttributeType attributeType, float value, float min = 0, float max = 0) : base(value, min, max)
        {
            AttributeType = attributeType;
        }

        public float CalculatedValue()
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