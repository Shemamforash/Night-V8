using System.Collections.Generic;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public abstract class AttributeContainer
    {
        private readonly Dictionary<AttributeType, CharacterAttribute> Attributes = new Dictionary<AttributeType, CharacterAttribute>();

        protected AttributeContainer()
        {
            CacheAttributes();
        }
        
        public CharacterAttribute Get(AttributeType type)
        {
            return Attributes.ContainsKey(type) ? Attributes[type] : null;
        }

        public float GetCalculatedValue(AttributeType type)
        {
            return Get(type).CurrentValue();
        }

        public void AddAttribute(CharacterAttribute a)
        {
            if (Attributes.ContainsKey(a.AttributeType)) throw new Exceptions.AttributeContainerAlreadyContainsAttributeException(a.AttributeType);
            Attributes[a.AttributeType] = a;
        }

        protected abstract void CacheAttributes();
    }
}