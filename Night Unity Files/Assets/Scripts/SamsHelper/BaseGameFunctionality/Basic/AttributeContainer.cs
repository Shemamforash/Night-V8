using System.Collections.Generic;
using System.Xml;
using SamsHelper.Persistence;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public abstract class AttributeContainer : IPersistenceTemplate
    {
        private readonly Dictionary<AttributeType, IntAttribute> _intAttributes = new Dictionary<AttributeType, IntAttribute>();
        private readonly Dictionary<AttributeType, FloatAttribute> _floatAttributes = new Dictionary<AttributeType, FloatAttribute>();

        public virtual void Load(XmlNode doc, PersistenceType saveType)
        {
            throw new System.NotImplementedException();
        }

        public virtual void Save(XmlNode doc, PersistenceType saveType)
        {
            throw new System.NotImplementedException();
        }

        public IAttribute Get(AttributeType type)
        {
            if (_intAttributes.ContainsKey(type))
            {
                return _intAttributes[type];
            }
            return _floatAttributes.ContainsKey(type) ? _floatAttributes[type] : null;
        }

        public float GetCalculatedValue(AttributeType type) => Get(type).GetCalculatedValue();

        protected void AddAttribute(IntAttribute a)
        {
            if (_intAttributes.ContainsKey(a.AttributeType))
            {
                throw new Exceptions.AttributeContainerAlreadyContainsAttributeException(a.AttributeType);
            }
            _intAttributes[a.AttributeType] = a;
        }

        protected void AddAttribute(FloatAttribute a)
        {
            if (_floatAttributes.ContainsKey(a.AttributeType))
            {
                throw new Exceptions.AttributeContainerAlreadyContainsAttributeException(a.AttributeType);
            }
            _floatAttributes[a.AttributeType] = a;
        }
    }
}