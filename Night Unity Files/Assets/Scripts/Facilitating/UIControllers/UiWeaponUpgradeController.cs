using System.Collections.Generic;
using Game.Characters;
using Game.Gear;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UiWeaponUpgradeController : UiGearMenuTemplate
    {
        private EnhancedText _damageText, _fireRateText;
        private EnhancedText _dpsText, _capacityText;

        private EnhancedButton _inscribeButton, _infuseButton;
        private EnhancedText _inscriptionText;
        private EnhancedText _reloadSpeedText, _accuracyText, _handlingText;

        private EnhancedText _nameText;
        private RectTransform _durabilityTransform;
        private ParticleSystem _durabilityParticles;
        private bool _upgradingAllowed;
        private EnhancedButton _weaponButton;
        private bool _showWeapons = true;

        public void Awake()
        {
            _durabilityTransform = Helper.FindChildWithName<RectTransform>(gameObject, "Max");
            _nameText = Helper.FindChildWithName<EnhancedText>(gameObject, "Name");
            _durabilityParticles = Helper.FindChildWithName<ParticleSystem>(gameObject, "Current");
            _damageText = Helper.FindChildWithName<EnhancedText>(gameObject, "Damage");
            _fireRateText = Helper.FindChildWithName<EnhancedText>(gameObject, "Fire Rate");
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
                if (!GearIsAvailable()) return;
                UiGearMenuController.Instance().EnableInput();
                _showWeapons = true;
                UiGearMenuController.Instance().SelectGear();
            });
            _inscribeButton.AddOnClick(() =>
            {
                if (!InscriptionsAreAvailable()) return;
                UiGearMenuController.Instance().EnableInput();
                _showWeapons = false;
                UiGearMenuController.Instance().SelectGear();
            });
        }

        private bool InscriptionsAreAvailable() => CharacterManager.Inscriptions.Count != 0;

        private void Infuse()
        {
            if (CurrentPlayer.EquippedWeapon == null) return;
            if (CurrentPlayer.EquippedWeapon.WeaponAttributes.GetDurability().ReachedMax()) return;
            if (WorldState.HomeInventory().GetResourceQuantity("Essence") == 0) return;
            WorldState.HomeInventory().DecrementResource("Essence", 1);
            CurrentPlayer.BrandManager.IncreaseEssenceInfused();
            int durabilityGain = 1 + (int)CurrentPlayer.Attributes.Val(AttributeType.EssenceRecoveryBonus);
            CurrentPlayer.EquippedWeapon.WeaponAttributes.IncreaseDurability(durabilityGain);
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

        public override List<InventoryItem> GetAvailableGear()
        {
            return _showWeapons ? new List<InventoryItem>(CharacterManager.Weapons) : new List<InventoryItem>(CharacterManager.Inscriptions);
        }

        public override void Equip(int selectedGear)
        {
            if (selectedGear == -1) return;
            if (_showWeapons)
            {
                CurrentPlayer.EquipWeapon(CharacterManager.Weapons[selectedGear]);
            }
            else
            {
                CurrentPlayer.EquippedWeapon.SetInscription(CharacterManager.Inscriptions[selectedGear]);
            }

            Show(CurrentPlayer);
        }

        public override Button GetGearButton() => _weaponButton.Button();

        public override void CompareTo(InventoryItem comparisonItem)
        {
            if (comparisonItem == null) return;
            Weapon compareWeapon = comparisonItem as Weapon;
            if (CurrentPlayer.EquippedWeapon == null)
            {
                SetWeaponInfo(compareWeapon);
            }
            else
            {
                _damageText.Text(GetAttributePrefix(compareWeapon, AttributeType.Damage) + " Damage");
                _fireRateText.Text(GetAttributePrefix(compareWeapon, AttributeType.FireRate) + " Fire Rate");
                _accuracyText.Text(GetAttributePrefix(compareWeapon, AttributeType.Accuracy) + "% Accuracy");
                _reloadSpeedText.Text(GetAttributePrefix(compareWeapon, AttributeType.ReloadSpeed) + "s Reload ");
                _handlingText.Text(GetAttributePrefix(compareWeapon, AttributeType.Handling) + "% Recoil Recovery");
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
            Weapon equipped = CurrentPlayer.EquippedWeapon;
            float equippedValue = equipped.GetAttributeValue(attribute);
            if (attribute == AttributeType.Capacity)
            {
                equippedValue = Mathf.FloorToInt(equippedValue);
            }

            string prefixString = Helper.Round(equippedValue, 1).ToString();
            if (compare == null) return prefixString;
            float compareValue = compare.GetAttributeValue(attribute);
            if (attribute == AttributeType.Capacity)
            {
                compareValue = Mathf.FloorToInt(compareValue);
            }

            prefixString = "<color=#505050>" + Helper.Round(compareValue, 1) + "</color>" + " vs " + prefixString;
            return prefixString;
        }

        private void UpdateDurabilityParticles()
        {
            float absoluteMaxDurability = ((int) ItemQuality.Radiant + 1) * 10;
            float maxDurability = ((int) CurrentPlayer.EquippedWeapon.Quality() + 1) * 10;
            float currentDurability = CurrentPlayer.EquippedWeapon.WeaponAttributes.GetDurability().CurrentValue();
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
            _damageText.Text(Helper.Round(attr.Val(AttributeType.Damage), 1) + " Dam");
            _fireRateText.Text(Helper.Round(attr.Val(AttributeType.FireRate), 1) + " RoF");
            _reloadSpeedText.Text(Helper.Round(attr.Val(AttributeType.ReloadSpeed), 1) + "s Reload");
            _handlingText.Text(Helper.Round(attr.Val(AttributeType.Handling), 1) + "% Handling");
            _dpsText.Text(Helper.Round(attr.DPS(), 1) + " DPS");
            _capacityText.Text(Helper.Round(attr.Val(AttributeType.Capacity), 1) + " Capacity");
        }

        private void SetTopToBottomNavigation(EnhancedButton button)
        {
            _weaponButton.SetDownNavigation(button);
            button.SetDownNavigation(UiGearMenuController.Instance()._closeButton);
        }

        private void SetNavigation()
        {
            bool infuseActive = _infuseButton.Button().interactable;
            SetTopToBottomNavigation(infuseActive ? _infuseButton : _inscribeButton);
            _inscribeButton.SetUpNavigation(_weaponButton, false);
            _inscribeButton.SetDownNavigation(UiGearMenuController.Instance()._closeButton, false);
        }

        private void SetWeapon()
        {
            Weapon weapon = CurrentPlayer.EquippedWeapon;
            if (weapon == null)
            {
                SetNoWeapon();
                return;
            }

            WeaponAttributes attr = weapon.WeaponAttributes;
            SetWeaponInfo(weapon);
            Inscription inscription = weapon.GetInscription();
            string inscriptionText = inscription != null ? inscription.GetSummary() : "No inscription";
            _inscriptionText.Text(inscriptionText);
            _infuseButton.Button().interactable = !attr.GetDurability().ReachedMax();

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

        public override void SelectGearItem(InventoryItem item, UiGearMenuController.GearUi gearUi)
        {
            Weapon weapon = item as Weapon;
            if (weapon != null)
            {
                Assert.IsTrue(_showWeapons);
                gearUi.SetTypeText(weapon.GetWeaponType());
                gearUi.SetNameText(weapon.Name);
                gearUi.SetDpsText(Helper.Round(weapon.WeaponAttributes.DPS(), 1) + " DPS");
                return;
            }

            Inscription inscription = item as Inscription;
            if (inscription == null) return;
            Assert.IsFalse(_showWeapons);
            gearUi.SetTypeText("");
            gearUi.SetNameText(inscription.GetSummary());
            gearUi.SetDpsText("");
        }
    }
}