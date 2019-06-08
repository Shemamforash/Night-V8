using Game.Characters;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;

namespace SamsHelper.BaseGameFunctionality.InventorySystem
{
	public class Consumable : ResourceItem
	{
		private          Player             _player;
		private          CharacterAttribute _attribute;
		private readonly bool               _isCoreAttribute;
		private readonly bool               _isPermanent;

		private Player Player
		{
			get => _player;
			set
			{
				_player    = value;
				_attribute = _player.Attributes.Get(Template.AttributeType);
			}
		}

		public Consumable(ResourceTemplate template) : base(template)
		{
			Template         = template;
			_isCoreAttribute = Template.AttributeType.IsCoreAttribute();
			_isPermanent     = template.IsEffectPermanent;
		}

		private void TryApplyEffect()
		{
			Player = CharacterManager.SelectedCharacter;
			if (!Template.HasEffect) return;
			ApplyEffect();
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

		private void ApplyEffect()
		{
			bool increaseMax = _isPermanent && _isCoreAttribute;
			if (!increaseMax) _attribute.CurrentValue += Template.EffectBonus;
			else Player.Attributes.IncreaseAttribute(Template.AttributeType);
		}

		public void Consume()
		{
			if (!CanConsume()) return;
			TryApplyEffect();
			Inventory.DecrementResource(Template.Name, 1);
			if (PlayerCombat.Instance == null) return;
			PlayerCombat.Instance.RecalculateAttributes();
		}

		public bool CanConsume()
		{
			Player = CharacterManager.SelectedCharacter;
			if (_isCoreAttribute && _isPermanent) return _attribute.Max != 20;
			return !_attribute.ReachedMax;
		}
	}
}