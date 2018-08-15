using System.Xml;
using Game.Characters;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Consumable : InventoryItem
    {
        private readonly bool _hasEffect;
        public readonly bool IsFood;
        public readonly bool IsWater;
        public int ThirstModifier;
        public int HungerModifier;

        public Consumable(ResourceTemplate template, Inventory parentInventory = null) : base(template, GameObjectType.Resource, parentInventory)
        {
            Template = template;
            IsFood = Template.ResourceType == "Food";
            IsWater = Template.ResourceType == "Water";
            AttributeModifier modifier = template.CreateModifier();
            _hasEffect = modifier != null;
            CalculateThirstOffset();
            CalculateHungerOffset();
        }

        private void CalculateThirstOffset()
        {
            if (IsWater) ThirstModifier = 1;
            if (!_hasEffect) return;
            if (Template.AttributeType == AttributeType.Thirst) ThirstModifier += (int) Template.ModifierVal;
        }

        private void CalculateHungerOffset()
        {
            if (IsFood) HungerModifier = 1;
            if (!_hasEffect) return;
            if (Template.AttributeType == AttributeType.Hunger) HungerModifier += (int) Template.ModifierVal;
        }

        public string Effect()
        {
            return !_hasEffect ? "" : Template.ModifierVal + " " + Template.AttributeType;
        }

        private void ApplyEffect(Player selectedCharacter)
        {
            switch (Template.ResourceType)
            {
                case "Food":
                    selectedCharacter.Attributes.Eat();
                    break;
                case "Water":
                    selectedCharacter.Attributes.Drink();
                    break;
            }

            if (!_hasEffect) return;
            CharacterAttribute attribute = selectedCharacter.Attributes.Get(Template.AttributeType);
            new Effect(selectedCharacter, Template.CreateModifier(), attribute, Template.Duration);
        }

        public void Consume(Player selectedCharacter)
        {
            ApplyEffect(selectedCharacter);
            ParentInventory().DecrementResource(Template.Name, 1);
        }

        public static Consumable LoadConsumable(XmlNode consumableNode)
        {
            //todo me
            return null;
        }
    }
}