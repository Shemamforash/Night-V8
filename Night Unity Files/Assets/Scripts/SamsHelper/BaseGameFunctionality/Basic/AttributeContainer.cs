﻿using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
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

        public virtual XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            XmlNode attributeNode = SaveController.CreateNodeAndAppend("Attributes", doc);
            foreach (AttributeType t in _attributes.Keys)
            {
                _attributes[t].Save(doc, saveType);
            }
            return attributeNode;
        }

        public CharacterAttribute Get(AttributeType type)
        {
            return _attributes.ContainsKey(type) ? _attributes[type] : null;
        }

        public float GetCalculatedValue(AttributeType type) => Get(type).CurrentValue();

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