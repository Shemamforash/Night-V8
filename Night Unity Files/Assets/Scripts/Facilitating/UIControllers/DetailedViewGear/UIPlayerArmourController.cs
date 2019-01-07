using System;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UIPlayerArmourController : MonoBehaviour
    {
        public EnhancedButton EnhancedButton;
        private EnhancedText _armourName, _armourBonus;
        private Player _player;

        public void Awake()
        {
            _armourName = gameObject.FindChildWithName<EnhancedText>("Armour Name");
            _armourBonus = gameObject.FindChildWithName<EnhancedText>("Bonus");
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
            _armourName.SetText(_player.ArmourController.GetName());
            _armourBonus.SetText(_player.ArmourController.GetBonus());
        }
    }
}