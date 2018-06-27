using System.Collections.Generic;
using System.Xml;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Persistence;

namespace Game.Global
{
    public class DesolationAttributes : IPersistenceTemplate
    {
        private readonly Dictionary<AttributeType, CharacterAttribute> _attributes = new Dictionary<AttributeType, CharacterAttribute>();
        
        private void AddAttribute(AttributeType attributeType)
        {
            _attributes.Add(attributeType, new CharacterAttribute(attributeType));
        }

        public void Set(AttributeType type, float value, float min = 0, float max = float.MaxValue)
        {
            CharacterAttribute attribute = Get(type);
            attribute.Min = 0;
            attribute.Max = max;
            attribute.SetCurrentValue(value);
        }
        
        public CharacterAttribute Get(AttributeType attributeType)
        {
            if (_attributes.ContainsKey(attributeType)) return _attributes[attributeType];
            AddAttribute(attributeType);
            return _attributes[attributeType];
        }

        public float Val(AttributeType attributeType)
        {
            return Get(attributeType).CurrentValue();
        }

        public void SetMin(AttributeType attributeType, float newMin)
        {
            Get(attributeType).Min = newMin;
        }

        public void SetMax(AttributeType attributeType, float newMax)
        {
            Get(attributeType).Max = newMax;
        }

        public float Min(AttributeType attributeType)
        {
            return Get(attributeType).Min;
        }

        public float Max(AttributeType attributeType)
        {
            return Get(attributeType).Max;
        }

        public void AddMod(AttributeType attributeType, AttributeModifier modifier)
        {
            Get(attributeType).AddModifier(modifier);
        }

        public void RemoveMod(AttributeType attributeType, AttributeModifier modifier)
        {
            Get(attributeType).RemoveModifier(modifier);
        }
        
        public void Load(XmlNode doc, PersistenceType saveType)
        {
        }

        public XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            return doc;
        }
    }
}