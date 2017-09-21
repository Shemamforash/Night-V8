using System.Collections.Generic;
using System.Xml;
using SamsHelper.Persistence;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public abstract class AttributeContainer : IPersistenceTemplate
    {
        private Dictionary<AttributeType, Attribute> _attributes = new Dictionary<AttributeType, Attribute>();
        
        public virtual void Load(XmlNode doc, PersistenceType saveType)
        {
            throw new System.NotImplementedException();
        }

        public virtual void Save(XmlNode doc, PersistenceType saveType)
        {
            throw new System.NotImplementedException();
        }

        public Attribute Get(AttributeType type)
        {
            return _attributes.ContainsKey(type) ? _attributes[type] : null;
        }

        public int GetCalculatedValue(AttributeType type)
        {
            return Get(type).CalculatedValue();
        }

        public void AddAttribute(Attribute a)
        {
            if (_attributes.ContainsKey(a.AttributeType))
            {
                throw new Exceptions.AttributeContainerAlreadyContainsAttributeException(a.AttributeType);
            }
            _attributes[a.AttributeType] = a;
        }
    }
}