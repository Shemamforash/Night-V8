using System.Collections.Generic;
using System.Xml;
using SamsHelper.Persistence;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public abstract class AttributeContainer : IPersistenceTemplate
    {
        private readonly Dictionary<AttributeType, CharacterAttribute> _attributes = new Dictionary<AttributeType, CharacterAttribute>();

        protected AttributeContainer()
        {
            CacheAttributes();
        }

        public virtual void Load(XmlNode doc, PersistenceType saveType)
        {
            throw new System.NotImplementedException();
        }

        public virtual void Save(XmlNode doc, PersistenceType saveType)
        {
            throw new System.NotImplementedException();
        }

        public CharacterAttribute Get(AttributeType type)
        {
            return _attributes.ContainsKey(type) ? _attributes[type] : null;
        }

        public float GetCalculatedValue(AttributeType type) => Get(type).GetCalculatedValue();

        protected abstract void CacheAttributes();
        
        protected void AddAttribute(CharacterAttribute a)
        {
            if (_attributes.ContainsKey(a.AttributeType))
            {
                throw new Exceptions.AttributeContainerAlreadyContainsAttributeException(a.AttributeType);
            }
            _attributes[a.AttributeType] = a;
        }
    }
}