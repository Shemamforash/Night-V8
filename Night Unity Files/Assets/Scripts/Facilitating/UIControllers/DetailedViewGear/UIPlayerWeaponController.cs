using System;
using Game.Characters;
using Game.Combat.Player;
using Game.Gear;
using Game.Gear.Weapons;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UIPlayerWeaponController : MonoBehaviour
    {
        private EnhancedText _nameText, _infusionText;
        private GameObject _equippedObject;
        public EnhancedButton EnhancedButton;
        private Player _player;

        public void Awake()
        {
            EnhancedButton = GetComponent<EnhancedButton>();
            _equippedObject = gameObject.FindChildWithName("Equipped");
            _nameText = _equippedObject.FindChildWithName<EnhancedText>("Weapon Name");
            _infusionText = _equippedObject.FindChildWithName<EnhancedText>("Bonus");
            EnhancedButton.AddOnClick(UiGearMenuController.ShowWeaponMenu);
            GlowButtonBehaviour glow = GetComponent<GlowButtonBehaviour>();
            EnhancedButton.AddOnClick(glow.Select);
            EnhancedButton.AddOnDeselectEvent(glow.Deselect);
            EnhancedButton.AddOnSelectEvent(glow.Highlight);
        }

        public void SetWeapon(Action selectAction, Player player)
        {
            EnhancedButton.AddOnSelectEvent(selectAction);
            _player = player;
            UpdateWeapon();
        }

        public void UpdateWeapon()
        {
            Weapon weapon = _player.EquippedWeapon;
            string weaponName = "";
            if (weapon != null) weaponName = weapon.Quality() + " " + weapon.WeaponAttributes.GetWeaponClass();
            _nameText.SetText(weaponName);

            string infusionText = "No Infusion";
            Inscription inscription = weapon?.GetInscription();
            if (inscription != null) infusionText = inscription.Name;
            _infusionText.SetText(infusionText);
        }
    }
}