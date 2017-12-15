using SamsHelper.ReactiveUI;
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
            AddOnValueChange(a => Recalculate());
        }

        public override float CurrentValue()
        {
            return _calculatedValue;
        }

        public float OriginalValue()
        {
            return base.CurrentValue();
        }

        private void Recalculate()
        {
            _calculatedValue = (OriginalValue() + _summativeModifier) * _multiplicativeModifier;
        }

        public void ApplySummativeModifier(float summativeModifier)
        {
            _summativeModifier += summativeModifier;
            Recalculate();
        }

        public void ApplyMultiplicativeModifier(float multiplicativeModifier)
        {
            _multiplicativeModifier += multiplicativeModifier;
            Recalculate();
        }

        public void RemoveSummativeModifier(float summativeModifier)
        {
            _summativeModifier -= summativeModifier;
            Recalculate();
        }

        public void RemoveMultiplicativeModifier(float multiplicativeModifier)
        {
            _multiplicativeModifier -= multiplicativeModifier;
            Recalculate();
        }

        public void SetToMax()
        {
            SetCurrentValue(Max);
        }
    }
}