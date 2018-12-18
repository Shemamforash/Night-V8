using Game.Characters;
using Game.Combat.Player;
using InventorySystem;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
    public class Consumable : ResourceItem
    {
        private Player _player;

        public Consumable(ResourceTemplate template) : base(template)
        {
            Template = template;
        }

        private void ApplyEffect()
        {
            _player = CharacterManager.SelectedCharacter;
            switch (Template.ResourceType)
            {
                case ResourceType.Meat:
                    _player.Attributes.Eat((int) Template.EffectBonus);
                    return;
                case ResourceType.Water:
                    _player.Attributes.Drink((int) Template.EffectBonus);
                    return;
            }


            if (!Template.HasEffect) return;
            if (Template.IsEffectPermanent) ApplyPermanentEffect();
            else ApplyImpermanentEffect();

            if (Template.AttributeType == AttributeType.Fettle && PlayerCombat.Instance != null) PlayerCombat.Instance.RecalculateHealth();
            if (Template.AttributeType == AttributeType.Focus && PlayerCombat.Instance != null) PlayerCombat.Instance.ResetCompass();
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
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.RecalculateAttributes();
        }

        public bool CanConsume()
        {
            _player = CharacterManager.SelectedCharacter;
            switch (Template.ResourceType)
            {
                case ResourceType.Meat:
                    return !_player.Attributes.Get(AttributeType.Hunger).ReachedMin();
                case ResourceType.Water:
                    return !_player.Attributes.Get(AttributeType.Thirst).ReachedMin();
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