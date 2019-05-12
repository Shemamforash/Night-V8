using System;
using Extensions;
using Game.Characters;

using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
	public class UIPlayerArmourController : MonoBehaviour
	{
		private EnhancedText   _armourName, _armourBonus;
		private Player         _player;
		public  EnhancedButton EnhancedButton;

		public void Awake()
		{
			_armourName    = gameObject.FindChildWithName<EnhancedText>("Armour Name");
			_armourBonus   = gameObject.FindChildWithName<EnhancedText>("Bonus");
			EnhancedButton = GetComponent<EnhancedButton>();
			EnhancedButton.AddOnClick(UiGearMenuController.ShowArmourMenu);
			GlowButtonBehaviour glow = GetComponent<GlowButtonBehaviour>();
			EnhancedButton.AddOnClick(glow.Select);
			EnhancedButton.AddOnDeselectEvent(glow.Deselect);
			EnhancedButton.AddOnSelectEvent(glow.Highlight);
		}

		public void SetArmour(Action selectCharacter, Player player)
		{
			_player = player;
			EnhancedButton.AddOnSelectEvent(selectCharacter);
			UpdateArmour();
		}

		public void UpdateArmour()
		{
			if (_armourName == null) return;
			_armourName.SetText(_player.Armour.Name);
			_armourBonus.SetText(_player.Armour.GetBonus());
		}
	}
}