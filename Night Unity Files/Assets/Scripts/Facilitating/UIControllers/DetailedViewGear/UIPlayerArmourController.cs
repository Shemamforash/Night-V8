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
        private UIArmourController _armourController;
        private Player _player;

        public void Awake()
        {
            _armourController = gameObject.FindChildWithName<UIArmourController>("Armour Bar");
            EnhancedButton = GetComponent<EnhancedButton>();
            EnhancedButton.AddOnClick(UiGearMenuController.ShowArmourMenu);
            GlowButtonBehaviour glow = GetComponent<GlowButtonBehaviour>();
            EnhancedButton.AddOnClick(glow.Select);
            EnhancedButton.AddOnDeselectEvent(glow.Deselect);
            EnhancedButton.AddOnSelectEvent(glow.Highlight);
        }

        public void UpdateArmour()
        {
            _armourController.UpdateArmour(_player.ArmourController);
        }

        public void SetArmour(Action selectCharacter, Player player)
        {
            _player = player;
            EnhancedButton.AddOnSelectEvent(selectCharacter);
            UpdateArmour();
        }
    }
}