namespace SamsHelper.BaseGameFunctionality.Basic
{
    public class AttributeModifier
    {
        private readonly float _modifierValue;

        public enum ModifierType
        {
            Additive,
            Multiplicative
        }

        public readonly ModifierType Type;
        
        private AttributeModifier(float modifierValue, ModifierType type)
        {
            Type = type;
            _modifierValue = modifierValue;
        }

        public void ApplyModifier(ref float value)
        {
            value += _modifierValue;
        }
    }
}