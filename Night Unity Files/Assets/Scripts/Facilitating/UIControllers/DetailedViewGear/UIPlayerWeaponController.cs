using System;
using Extensions;
using Game.Characters;
using Game.Gear;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Facilitating.UIControllers
{
    public class UIPlayerWeaponController : MonoBehaviour
    {
        private EnhancedText _nameText, _inscriptionText;
        private GameObject _equippedObject;
        public EnhancedButton EnhancedButton;
        private Player _player;

        public void Awake()
        {
            EnhancedButton = GetComponent<EnhancedButton>();
            _equippedObject = gameObject.FindChildWithName("Equipped");
            _nameText = _equippedObject.FindChildWithName<EnhancedText>("Weapon Name");
            _inscriptionText = _equippedObject.FindChildWithName<EnhancedText>("Bonus");
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

            string inscriptionText = "No Inscription";
            Inscription inscription = weapon?.GetInscription();
            if (inscription != null) inscriptionText = inscription.Name;
            _inscriptionText.SetText(inscriptionText);
        }
    }
}