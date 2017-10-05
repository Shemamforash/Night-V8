using System.Collections.Generic;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public class AttributesModifier
    {
        private readonly Dictionary<AttributeType, float> _summativeModifiers = new Dictionary<AttributeType, float>();
        private readonly Dictionary<AttributeType, float> _multiplicativeModifiers = new Dictionary<AttributeType, float>();
        private bool _applied;
        
        public void Apply(AttributeContainer attributes)
        {
            if (_applied) return;
            foreach (AttributeType attributeType in _summativeModifiers.Keys)
            {
                attributes.Get(attributeType).AddModifier(_summativeModifiers[attributeType], true);
            }
            foreach (AttributeType attributeType in _multiplicativeModifiers.Keys)
            {
                attributes.Get(attributeType).AddModifier(_multiplicativeModifiers[attributeType]);
            }
            _applied = true;
        }

        public void Remove(AttributeContainer attributes)
        {
            if (!_applied) return;
            foreach (AttributeType attributeType in _summativeModifiers.Keys)
            {
                attributes.Get(attributeType).RemoveModifier(_summativeModifiers[attributeType], true);
            }
            foreach (AttributeType attributeType in _multiplicativeModifiers.Keys)
            {
                attributes.Get(attributeType).RemoveModifier(_multiplicativeModifiers[attributeType]);
            }
            _applied = false;
        }

        public void AddModifier(AttributeType type, float value, bool summative = false)
        {
            if (summative) _summativeModifiers[type] = value;
            else _multiplicativeModifiers[type] = value;
        }
    }
}