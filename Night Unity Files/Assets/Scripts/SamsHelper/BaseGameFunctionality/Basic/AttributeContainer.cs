using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using SamsHelper.Persistence;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public abstract class AttributeContainer : IPersistenceTemplate
    {
        public readonly Dictionary<AttributeType, CharacterAttribute> Attributes = new Dictionary<AttributeType, CharacterAttribute>();

        protected AttributeContainer()
        {
            CacheAttributes();
        }

        public virtual void Load(XmlNode doc, PersistenceType saveType)
        {
            throw new System.NotImplementedException();
        }

        public virtual XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            XmlNode attributeNode = SaveController.CreateNodeAndAppend("Attributes", doc);
            foreach (AttributeType t in Attributes.Keys)
            {
                Attributes[t].Save(doc, saveType);
            }
            return attributeNode;
        }

        public CharacterAttribute Get(AttributeType type)
        {
            return Attributes.ContainsKey(type) ? Attributes[type] : null;
        }

        public float GetCalculatedValue(AttributeType type) => Get(type).CurrentValue();

        protected abstract void CacheAttributes();
        
        protected void AddAttribute(CharacterAttribute a)
        {
            if (Attributes.ContainsKey(a.AttributeType))
            {
                throw new Exceptions.AttributeContainerAlreadyContainsAttributeException(a.AttributeType);
            }
            Attributes[a.AttributeType] = a;
        }
    }
}