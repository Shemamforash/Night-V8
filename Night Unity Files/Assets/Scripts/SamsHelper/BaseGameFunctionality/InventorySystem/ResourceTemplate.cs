using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class ResourceTemplate
    {
        public readonly string Name, ResourceType;
        public float Duration;
        private readonly bool Consumable;
        public AttributeType AttributeType;
        public float ModifierVal;
        private bool _additive;
        private bool _hasEffect;

        public ResourceTemplate(string name, string type) : this(name)
        {
            ResourceType = type;
            Consumable = true;
        }

        public ResourceTemplate(string name)
        {
            Name = name;
        }

        public InventoryItem Create()
        {
            InventoryItem item = Consumable ? new Consumable(this) : new InventoryItem(this, GameObjectType.Resource);
            item.SetStackable(true);
            return item;
        }

        public void SetEffect(AttributeType attribute, float modifierVal, bool additive, int duration)
        {
            AttributeType = attribute;
            ModifierVal = modifierVal;
            _additive = additive;
            Duration = duration;
            _hasEffect = true;
        }

        public AttributeModifier CreateModifier()
        {
            if (!_hasEffect) return null; 
            AttributeModifier attributeModifier = new AttributeModifier();
            if(_additive)
                attributeModifier.SetRawBonus(ModifierVal);
            else 
                attributeModifier.SetFinalBonus(ModifierVal);
            return attributeModifier;
        }
    }
}