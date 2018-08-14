using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
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

        public void SetVal(AttributeType type, float value)
        {
            CharacterAttribute attribute = Get(type);
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
        
        public void Load(XmlNode doc)
        {
        }

        public virtual XmlNode Save(XmlNode doc)
        {
            doc = doc.CreateChild("Attributes");
            foreach (KeyValuePair<AttributeType, CharacterAttribute> attribute in _attributes)
            {
                doc.CreateChild("AttributeType", attribute.Key.ToString());
                attribute.Value.Save(doc);
            }

            return doc;
        }
    }
}