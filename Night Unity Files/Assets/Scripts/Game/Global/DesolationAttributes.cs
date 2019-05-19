using System.Collections.Generic;
using System.Xml;
using Extensions;
using Facilitating.Persistence;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Global
{
	public class DesolationAttributes
	{
		private readonly Dictionary<AttributeType, CharacterAttribute> _attributes = new Dictionary<AttributeType, CharacterAttribute>();

		private void AddAttribute(AttributeType attributeType)
		{
			_attributes.Add(attributeType, new CharacterAttribute());
		}

		public float Val(AttributeType    attributeType)               => Get(attributeType).CurrentValue;
		public void  SetMax(AttributeType attributeType, float newMax) => Get(attributeType).Max = newMax;

		public CharacterAttribute Get(AttributeType attributeType)
		{
			if (_attributes.ContainsKey(attributeType)) return _attributes[attributeType];
			AddAttribute(attributeType);
			return _attributes[attributeType];
		}

		public float Max(AttributeType attributeType) => Get(attributeType).Max;

		public virtual void Load(XmlNode doc)
		{
			XmlNode attributesNode = doc.SelectSingleNode("Attributes");
			foreach (XmlNode attributeNode in attributesNode.SelectNodes("Attribute"))
			{
				AttributeType attributeType = (AttributeType) attributeNode.ParseInt("AttributeType");
				Get(attributeType).Load(attributeNode);
			}
		}

		public virtual XmlNode Save(XmlNode doc)
		{
			doc = doc.CreateChild("Attributes");
			foreach (KeyValuePair<AttributeType, CharacterAttribute> attribute in _attributes)
			{
				XmlNode attributeNode = doc.CreateChild("Attribute");
				attributeNode.CreateChild("AttributeType", (int) attribute.Key);
				attribute.Value.Save(attributeNode);
			}

			return doc;
		}
	}
}