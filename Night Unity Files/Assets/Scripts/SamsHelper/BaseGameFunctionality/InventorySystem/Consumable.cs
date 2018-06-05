using System;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Consumable : InventoryItem
    {
        private readonly Tuple<AttributeType, AttributeModifier, float> _effect1, _effect2;
        
        public Consumable(ResourceTemplate template, GameObjectType type, Inventory parentInventory = null) : base(template, type, parentInventory)
        {
            GenerateEffect(template.Effect1, ref _effect1, template.Duration1);
            GenerateEffect(template.Effect2, ref _effect2, template.Duration2);
        }

        private void GenerateEffect(string effectString, ref Tuple<AttributeType, AttributeModifier, float> effect, float duration)
        {
            if (effectString == "") return;
            string[] splitModifier = effectString.Split(' ');
            float amount = float.Parse(splitModifier[0]);
            AttributeType target = StringToAttribute(splitModifier[1]);
            AttributeModifier modifier = new AttributeModifier();
            modifier.SetRawBonus(amount);
            effect = Tuple.Create(target, modifier, duration);
        }

        public string Effect1()
        {
            return _effect1 == null ? "" : _effect1.Item2.RawBonusToString() + " " + _effect1.Item1;
        }

        public string Effect2()
        {
            return _effect2 == null ? "" : _effect2.Item2.RawBonusToString() + " " + _effect2.Item1;
        }
        
        private AttributeType StringToAttribute(string attributeString)
        {
            foreach (AttributeType attribute in Enum.GetValues(typeof(AttributeType)))
            {
                if (attribute.ToString() == attributeString)
                {
                    return attribute;
                }
            }
            throw new Exceptions.AttributeNotRecognisedException(attributeString);
        }

        private void ApplyEffect(Tuple<AttributeType, AttributeModifier, float> effect, Player selectedCharacter)
        {
            if (effect == null) return;
            CharacterAttribute attribute = selectedCharacter.Attributes.Get(effect.Item1);
            new Effect(selectedCharacter, effect.Item2, attribute, effect.Item3);
        }
        
        public void Consume(Player selectedCharacter)
        {
            ApplyEffect(_effect1, selectedCharacter);
            ApplyEffect(_effect2, selectedCharacter);
            Decrement(1);
        }
    }
}