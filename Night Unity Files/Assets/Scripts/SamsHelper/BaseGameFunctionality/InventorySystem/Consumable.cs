using System.Xml;
using Game.Characters;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Consumable : InventoryItem
    {
        private readonly bool IsFood;
        private readonly bool IsWater;
        public int ThirstModifier;
        public int HungerModifier;

        public Consumable(ResourceTemplate template, Inventory parentInventory = null) : base(template, GameObjectType.Resource, parentInventory)
        {
            Template = template;
            IsFood = Template.ResourceType == "Food";
            IsWater = Template.ResourceType == "Water";
            CalculateThirstOffset();
            CalculateHungerOffset();
        }

        private void CalculateThirstOffset()
        {
            if (IsWater) ThirstModifier = 1;
            if (!Template.HasEffect) return;
            if (Template.AttributeType == AttributeType.Thirst) ThirstModifier += (int) Template.EffectBonus;
        }

        private void CalculateHungerOffset()
        {
            if (IsFood) HungerModifier = 1;
            if (!Template.HasEffect) return;
            if (Template.AttributeType == AttributeType.Hunger) HungerModifier += (int) Template.EffectBonus;
        }

        public string Effect()
        {
            return !Template.HasEffect ? "" : Template.EffectBonus + " " + Template.AttributeType;
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

            if (!Template.HasEffect) return;
            CharacterAttribute attribute = selectedCharacter.Attributes.Get(Template.AttributeType);
            attribute.Increment(Template.EffectBonus);
        }

        public void Consume(Player selectedCharacter)
        {
            ApplyEffect(selectedCharacter);
            ParentInventory().DecrementResource(Template.Name, 1);
        }
    }
}