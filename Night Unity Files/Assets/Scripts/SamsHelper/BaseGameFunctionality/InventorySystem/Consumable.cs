using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Consumable : InventoryItem
    {
        private readonly bool _hasEffect;
        private readonly ResourceTemplate _template;
        public readonly bool IsFood;
        public readonly bool IsWater;
        public int ThirstModifier;
        public int HungerModifier;

        public Consumable(ResourceTemplate template, Inventory parentInventory = null) : base(template, GameObjectType.Resource, parentInventory)
        {
            _template = template;
            IsFood = _template.ResourceType == "Food";
            IsWater = _template.ResourceType == "Water";
            AttributeModifier modifier = template.CreateModifier();
            _hasEffect = modifier != null;
            CalculateThirstOffset();
            CalculateHungerOffset();
        }

        private void CalculateThirstOffset()
        {
            if (IsWater) ThirstModifier = 1;
            if (!_hasEffect) return;
            if (_template.AttributeType == AttributeType.Thirst) ThirstModifier += (int) _template.ModifierVal;
        }

        private void CalculateHungerOffset()
        {
            if (IsFood) HungerModifier = 1;
            if (!_hasEffect) return;
            if (_template.AttributeType == AttributeType.Hunger) HungerModifier += (int) _template.ModifierVal;
        }

        public string Effect()
        {
            return !_hasEffect ? "" : _template.ModifierVal + " " + _template.AttributeType;
        }

        private void ApplyEffect(Player selectedCharacter)
        {
            switch (_template.ResourceType)
            {
                case "Food":
                    selectedCharacter.Attributes.Eat();
                    break;
                case "Water":
                    selectedCharacter.Attributes.Drink();
                    break;
            }

            if (!_hasEffect) return;
            CharacterAttribute attribute = selectedCharacter.Attributes.Get(_template.AttributeType);
            new Effect(selectedCharacter, _template.CreateModifier(), attribute, _template.Duration);
        }

        public void Consume(Player selectedCharacter)
        {
            ApplyEffect(selectedCharacter);
            ParentInventory.DecrementResource(_template.Name, 1);
        }
    }
}