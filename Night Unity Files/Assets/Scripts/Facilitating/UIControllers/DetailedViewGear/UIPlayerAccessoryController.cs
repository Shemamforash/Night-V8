using System;
using Extensions;
using Game.Characters;
using Game.Gear.Armour;

using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
	public class UIPlayerAccessoryController : MonoBehaviour
	{
		private EnhancedText   _accessoryText, _bonusText;
		private Player         _player;
		public  EnhancedButton EnhancedButton;

		public void Awake()
		{
			EnhancedButton = GetComponent<EnhancedButton>();
			_accessoryText = gameObject.FindChildWithName<EnhancedText>("Accessory Name");
			_bonusText     = gameObject.FindChildWithName<EnhancedText>("Bonus");

			EnhancedButton.AddOnClick(UiGearMenuController.ShowAccessoryMenu);
			GlowButtonBehaviour glow = GetComponent<GlowButtonBehaviour>();
			EnhancedButton.AddOnClick(glow.Select);
			EnhancedButton.AddOnDeselectEvent(glow.Deselect);
			EnhancedButton.AddOnSelectEvent(glow.Highlight);
		}

		public void SetAccessory(Action selectAction, Player player)
		{
			EnhancedButton.AddOnSelectEvent(selectAction);
			_player = player;
			UpdateAccessory();
		}

		public void UpdateAccessory()
		{
			Accessory accessory = _player.Accessory;
			_accessoryText.SetText(accessory == null ? "" : accessory.Name);
			_bonusText.SetText(accessory     == null ? "" : accessory.GetSummary());
		}
	}
}