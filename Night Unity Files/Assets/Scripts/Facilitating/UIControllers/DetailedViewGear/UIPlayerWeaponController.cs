using System;
using Extensions;
using Game.Characters;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UIPlayerWeaponController : MonoBehaviour
    {
        private EnhancedText _nameText;
        private GameObject _equippedObject;
        public EnhancedButton EnhancedButton;
        private Player _player;

        public void Awake()
        {
            EnhancedButton = GetComponent<EnhancedButton>();
            _equippedObject = gameObject.FindChildWithName("Equipped");
            _nameText = _equippedObject.FindChildWithName<EnhancedText>("Weapon Name");
            EnhancedButton.AddOnClick(UiGearMenuController.ShowWeaponMenu);
            GlowButtonBehaviour glow = GetComponent<GlowButtonBehaviour>();
            EnhancedButton.AddOnClick(glow.Select);
            EnhancedButton.AddOnDeselectEvent(glow.Deselect);
            EnhancedButton.AddOnSelectEvent(glow.Highlight);
#if UNITY_EDITOR
            ResourceTemplate.AllResources.ForEach(r => { Inventory.IncrementResource(r.Name, 10); });
            Inventory.IncrementResource("Essence", 38);
#endif
        }

        public void SetWeapon(Action selectAction, Player player)
        {
            EnhancedButton.AddOnSelectEvent(selectAction);
            _player = player;
            UpdateWeapon();
        }

        public void UpdateWeapon()
        {
            Weapon weapon = _player.Weapon;
            string weaponName = "";
            if (weapon != null) weaponName = weapon.Quality() + " " + weapon.WeaponAttributes.GetWeaponClass();
            _nameText.SetText(weaponName);
        }
    }
}