﻿using System.Collections.Generic;
using Game.Characters;
using Game.Gear;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UiWeaponUpgradeController : UiGearMenuTemplate
    {
        private EnhancedText _damageText, _fireRateText, _rangeText;
        private EnhancedText _dpsText, _capacityText;

        private EnhancedButton _inscribeButton, _infuseButton;
        private EnhancedText _inscriptionText;
        private EnhancedText _reloadSpeedText, _accuracyText, _handlingText;

        private EnhancedText _nameText;
        private RectTransform _durabilityTransform;
        private ParticleSystem _durabilityParticles;
        private bool _upgradingAllowed;
        private EnhancedButton _weaponButton;

        public void Awake()
        {
            _durabilityTransform = Helper.FindChildWithName<RectTransform>(gameObject, "Max");
            _nameText = Helper.FindChildWithName<EnhancedText>(gameObject, "Name");
            _durabilityParticles = Helper.FindChildWithName<ParticleSystem>(gameObject, "Current");
            _damageText = Helper.FindChildWithName<EnhancedText>(gameObject, "Damage");
            _fireRateText = Helper.FindChildWithName<EnhancedText>(gameObject, "Fire Rate");
            _rangeText = Helper.FindChildWithName<EnhancedText>(gameObject, "Range");
            _dpsText = Helper.FindChildWithName<EnhancedText>(gameObject, "DPS");
            _capacityText = Helper.FindChildWithName<EnhancedText>(gameObject, "Capacity");
            _reloadSpeedText = Helper.FindChildWithName<EnhancedText>(gameObject, "Reload Speed");
            _accuracyText = Helper.FindChildWithName<EnhancedText>(gameObject, "Critical Chance");
            _handlingText = Helper.FindChildWithName<EnhancedText>(gameObject, "Handling");
            _inscriptionText = Helper.FindChildWithName<EnhancedText>(gameObject, "Inscription");

            _inscribeButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Inscribe");
            _infuseButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Infuse");
            _infuseButton.AddOnClick(Infuse);

            _weaponButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Info");
            _weaponButton.AddOnClick(() =>
            {
                if (GearIsAvailable()) UiGearMenuController.Instance().EnableInput();
            });
        }

        private void Infuse()
        {
            if (CurrentPlayer.Weapon == null) return;
            if (CurrentPlayer.Weapon.WeaponAttributes.Durability.ReachedMax()) return;
            if (WorldState.HomeInventory().GetResourceQuantity("Essence") == 0) return;
            WorldState.HomeInventory().DecrementResource("Essence", 1);
            CurrentPlayer.Weapon.WeaponAttributes.Durability.Increment();
            UpdateDurabilityParticles();
        }

        public override void Show(Player player)
        {
            base.Show(player);
            SetWeapon();
        }

        public override void StopComparing()
        {
            SetWeapon();
        }

        public override List<GearItem> GetAvailableGear() => new List<GearItem>(CharacterManager.Weapons);

        public override void Equip(int selectedGear)
        {
            if (selectedGear == -1) return;
            CurrentPlayer.EquipWeapon(CharacterManager.Weapons[selectedGear]);
            Show(CurrentPlayer);
        }

        public override Button GetGearButton() => _weaponButton.Button();

        public override void CompareTo(GearItem comparisonItem)
        {
            if (comparisonItem == null) return;
            Weapon compareWeapon = comparisonItem as Weapon;
            if (CurrentPlayer.Weapon == null)
            {
                SetWeaponInfo(compareWeapon);
            }
            else
            {
                _damageText.Text(GetAttributePrefix(compareWeapon, AttributeType.Damage) + " Dam");
                _fireRateText.Text(GetAttributePrefix(compareWeapon, AttributeType.FireRate) + " RoF");
                _accuracyText.Text(GetAttributePrefix(compareWeapon, AttributeType.Accuracy) + "%");
                _reloadSpeedText.Text(GetAttributePrefix(compareWeapon, AttributeType.ReloadSpeed) + "s Reload ");
                _handlingText.Text(GetAttributePrefix(compareWeapon, AttributeType.Handling) + "% Handling ");
                _capacityText.Text(GetAttributePrefix(compareWeapon, AttributeType.Capacity) + " Capacity");
            }
        }

        public override void Hide()
        {
            base.Hide();
            _durabilityParticles.Stop();
        }

        private string GetAttributePrefix(Weapon compare, AttributeType attribute)
        {
            Weapon equipped = CurrentPlayer.Weapon;
            float equippedValue = equipped.GetAttributeValue(attribute);
            float compareValue = compare.GetAttributeValue(attribute);
            return "<color=#505050>" + Helper.Round(compareValue, 1) + "</color>" + " vs " + Helper.Round(equippedValue, 1);
        }

        private void UpdateDurabilityParticles()
        {
            float absoluteMaxDurability = ((int) ItemQuality.Radiant + 1) * 10;
            float maxDurability = ((int) CurrentPlayer.Weapon.Quality() + 1) * 10;
            float currentDurability = CurrentPlayer.Weapon.WeaponAttributes.Durability.CurrentValue();
            float rectAnchorOffset = maxDurability / absoluteMaxDurability / 2;
            float particleOffset = 5.6f * (currentDurability / absoluteMaxDurability);
            _durabilityTransform.anchorMin = new Vector2(0.5f - rectAnchorOffset, 0.5f);
            _durabilityTransform.anchorMax = new Vector2(0.5f + rectAnchorOffset, 0.5f);
            ParticleSystem.ShapeModule shape = _durabilityParticles.shape;
            shape.radius = particleOffset;
            ParticleSystem.EmissionModule emission = _durabilityParticles.emission;
            emission.rateOverTime = 300 * particleOffset / 5.6f;
            _durabilityParticles.Play();
        }
        
        private void SetWeaponInfo(Weapon weapon)
        {
            WeaponAttributes attr = weapon.WeaponAttributes;
            _nameText.Text(weapon.Name);
            UpdateDurabilityParticles();
            _damageText.Text(Helper.Round(attr.Damage.CurrentValue(), 1) + " Dam");
            _fireRateText.Text(Helper.Round(attr.FireRate.CurrentValue(), 1) + " RoF");
            _reloadSpeedText.Text(Helper.Round(attr.ReloadSpeed.CurrentValue(), 1) + "s Reload");
            _handlingText.Text(Helper.Round(attr.Handling.CurrentValue(), 1) + "% Handling");
            _dpsText.Text(Helper.Round(attr.DPS(), 1) + " DPS");
            _capacityText.Text(Helper.Round(attr.Capacity.CurrentValue(), 1) + " Capacity");
        }

        private void SetTopToBottomNavigation(EnhancedButton button)
        {
            _weaponButton.SetDownNavigation(button);
            button.SetDownNavigation(UiGearMenuController.Instance()._closeButton);
        }

        private void SetNavigation()
        {
            bool inscribeActive = _inscribeButton.Button().interactable;
            bool infuseActive = _infuseButton.Button().interactable;

            if (infuseActive)
            {
                SetTopToBottomNavigation(_infuseButton);
            }
            else if (inscribeActive)
            {
                SetTopToBottomNavigation(_inscribeButton);
            }
            else
            {
                _weaponButton.SetDownNavigation(UiGearMenuController.Instance()._closeButton);
            }

            if (inscribeActive)
            {
                _inscribeButton.SetUpNavigation(_weaponButton, false);
                _inscribeButton.SetDownNavigation(UiGearMenuController.Instance()._closeButton, false);
            }
        }

        private void SetWeapon()
        {
            Weapon weapon = CurrentPlayer.Weapon;
            if (weapon == null)
            {
                SetNoWeapon();
                return;
            }

            WeaponAttributes attr = weapon.WeaponAttributes;
            SetWeaponInfo(weapon);
            _inscribeButton.Button().interactable = weapon.Inscribable();
            _inscriptionText.Text(weapon.Inscribable() ? "-- Insert Inscription --" : "-- Not Inscribable --");
            _infuseButton.Button().interactable = !attr.Durability.ReachedMax();

            SetNavigation();
        }

        private void SetNoWeaponInfo()
        {
            _durabilityTransform.anchorMin = Vector2.zero;
            _durabilityTransform.anchorMax = Vector2.zero;
            _nameText.Text("");
            ParticleSystem.EmissionModule emission = _durabilityParticles.emission;
            emission.rateOverTime = 0f;
            _damageText.Text("");
            _fireRateText.Text("");
            _rangeText.Text("");
            _dpsText.Text("Nothing Equipped");
            _capacityText.Text("");
            _reloadSpeedText.Text("");
            _accuracyText.Text("");
            _handlingText.Text("");
            _inscriptionText.Text("");
        }

        private void SetNoWeapon()
        {
            SetNoWeaponInfo();
            _inscribeButton.Button().interactable = false;
            _infuseButton.Button().interactable = false;
            SetNavigation();
        }

        public override bool GearIsAvailable() => CharacterManager.Weapons.Count != 0;

        public override void SelectGearItem(GearItem item, UiGearMenuController.GearUi gearUi)
        {
            Weapon weapon = item as Weapon;
            gearUi.SetTypeText(weapon.GetWeaponType());
            gearUi.SetNameText(weapon.Name);
            gearUi.SetDpsText(Helper.Round(weapon.WeaponAttributes.DPS(), 1) + " DPS");
        }
    }
}