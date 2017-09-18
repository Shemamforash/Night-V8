using System.Collections.Generic;

namespace Game.Gear
{
    public abstract class AttributeModifier
    {
        private readonly Dictionary<AttributeType, float> _attributeModifiers = new Dictionary<AttributeType, float>();

        public virtual float ApplyModifier(float value, AttributeType attributeType)
        {
            if (_attributeModifiers.ContainsKey(attributeType))
            {
                return value += _attributeModifiers[attributeType];
            }
            return value;
        }
    }
}