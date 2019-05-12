using Game.Characters;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
	public class Consumable : ResourceItem
	{
		private Player _player;

		public Consumable(ResourceTemplate template) : base(template) => Template = template;

		private void ApplyEffect()
		{
			_player = CharacterManager.SelectedCharacter;
			if (!Template.HasEffect) return;
			if (Template.IsEffectPermanent)
				ApplyPermanentEffect();
			else
				ApplyImpermanentEffect();

			if (!PlayerCombat.Instance) return;
			switch (Template.AttributeType)
			{
				case AttributeType.Life:
					PlayerCombat.Instance.RecalculateHealth();
					break;
				case AttributeType.Will:
					PlayerCombat.Instance.ResetCompass();
					break;
			}
		}

		private void ApplyPermanentEffect()
		{
			CharacterAttribute attribute = _player.Attributes.Get(Template.AttributeType);
			if (!CharacterAttribute.IsCharacterAttribute(Template.AttributeType))
			{
				attribute.Increment(Template.EffectBonus);
				return;
			}

			_player.Attributes.IncreaseAttribute(Template.AttributeType);
		}

		private void ApplyImpermanentEffect()
		{
			CharacterAttribute attribute = _player.Attributes.Get(Template.AttributeType);
			attribute.Increment(Template.EffectBonus);
		}

		public void Consume()
		{
			if (!CanConsume()) return;
			if (Template.Name == "Mystic Shard")
			{
				Inventory.DecrementResource(Template.Name, 1);
				_player.TravelAction.ReturnToHomeInstant(true);
				return;
			}

			ApplyEffect();
			Inventory.DecrementResource(Template.Name, 1);
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.RecalculateAttributes();
		}

		public bool CanConsume()
		{
			_player = CharacterManager.SelectedCharacter;
			if (Template.Name == "Mystic Shard") return !_player.TravelAction.AtHome();
			return Template.IsEffectPermanent ? CheckCanConsumePermanent() : CheckCanConsumeImpermanent();
		}

		private bool CheckCanConsumeImpermanent() => !_player.Attributes.Get(Template.AttributeType).ReachedMax;

		private bool CheckCanConsumePermanent()
		{
			CharacterAttribute attribute = _player.Attributes.Get(Template.AttributeType);
			if (CharacterAttribute.IsCharacterAttribute(Template.AttributeType)) return attribute.Max != 20;
			return !attribute.ReachedMax && !attribute.ReachedMin;
		}
	}
}