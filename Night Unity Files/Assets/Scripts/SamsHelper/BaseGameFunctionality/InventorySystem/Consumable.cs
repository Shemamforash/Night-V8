﻿using Game.Characters;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Consumable : InventoryItem
    {
        private Player _player;

        public Consumable(ResourceTemplate template) : base(template, GameObjectType.Resource)
        {
            Template = template;
        }

        private void ApplyEffect()
        {
            _player = CharacterManager.SelectedCharacter;
            Debug.Log(Template.EffectBonus);
            switch (Template.ResourceType)
            {
                case "Meat":
                    _player.Attributes.Eat((int) Template.EffectBonus);
                    break;
                case "Water":
                    _player.Attributes.Drink((int) Template.EffectBonus);
                    break;
            }

            if (!Template.HasEffect) return;
            if (Template.IsEffectPermanent) ApplyPermanentEffect();
            else ApplyImpermanentEffect();
        }

        private void ApplyPermanentEffect()
        {
            CharacterAttribute attribute = _player.Attributes.Get(Template.AttributeType);
            if (!CharacterAttribute.IsCharacterAttribute(Template.AttributeType))
            {
                if (Template.EffectBonus < 0) attribute.Decrement(-Template.EffectBonus);
                else attribute.Increment(Template.EffectBonus);
                return;
            }

            switch (Template.AttributeType)
            {
                case AttributeType.Fettle:
                    _player.Attributes.ChangeFettleMax((int) Template.EffectBonus);
                    break;
                case AttributeType.Grit:
                    _player.Attributes.ChangeGritMax((int) Template.EffectBonus);
                    break;
                case AttributeType.Will:
                    _player.Attributes.ChangeWillMax((int) Template.EffectBonus);
                    break;
                case AttributeType.Focus:
                    _player.Attributes.ChangeFocusMax((int) Template.EffectBonus);
                    break;
            }
        }

        private void ApplyImpermanentEffect()
        {
            CharacterAttribute attribute = _player.Attributes.Get(Template.AttributeType);
            if (Template.EffectBonus > 0) attribute.Increment(Template.EffectBonus);
            else attribute.Decrement(-Template.EffectBonus);
        }

        public void Consume()
        {
            if (!CanConsume()) return;
            ApplyEffect();
            Inventory.DecrementResource(Template.Name, 1);
        }

        public bool CanConsume()
        {
            _player = CharacterManager.SelectedCharacter;
            switch (Template.ResourceType)
            {
                case "Meat":
                    return !_player.Attributes.Get(AttributeType.Hunger).ReachedMax();
                case "Water":
                    return !_player.Attributes.Get(AttributeType.Thirst).ReachedMax();
            }

            return Template.IsEffectPermanent ? CheckCanConsumePermanent() : CheckCanConsumeImpermanent();
        }

        private bool CheckCanConsumeImpermanent()
        {
            return !_player.Attributes.Get(Template.AttributeType).ReachedMax();
        }

        private bool CheckCanConsumePermanent()
        {
            CharacterAttribute attribute = _player.Attributes.Get(Template.AttributeType);
            if (CharacterAttribute.IsCharacterAttribute(Template.AttributeType)) return attribute.Max != 20;
            return true;
        }
    }
}