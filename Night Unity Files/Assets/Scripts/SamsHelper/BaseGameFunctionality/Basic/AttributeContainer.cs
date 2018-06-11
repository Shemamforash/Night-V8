using System.Collections.Generic;

namespace SamsHelper.BaseGameFunctionality.Basic
{
    public abstract class AttributeContainer
    {
        private readonly Dictionary<AttributeType, CharacterAttribute> Attributes = new Dictionary<AttributeType, CharacterAttribute>();

        protected AttributeContainer()
        {
            CacheAttributes();
        }

        public CharacterAttribute Get(AttributeType type) => Attributes.ContainsKey(type) ? Attributes[type] : null;

        public float GetCalculatedValue(AttributeType type) => Get(type).CurrentValue();

        public void AddAttribute(CharacterAttribute a)
        {
            if (Attributes.ContainsKey(a.AttributeType)) throw new Exceptions.AttributeContainerAlreadyContainsAttributeException(a.AttributeType);
            Attributes[a.AttributeType] = a;
        }

        protected abstract void CacheAttributes();
    }
}