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

        private EnhancedButton _inscribeButton, _infuseButton, _swapButton;
        private EnhancedText _inscriptionText;
        private EnhancedText _reloadSpeedText, _accuracyText, _handlingText;

        private EnhancedText _nameText;
        private RectTransform _durabilityTransform;
        private ParticleSystem _durabilityParticles;
        private bool _upgradingAllowed;
        private bool _showWeapons = true;

        public void Awake()
        {
            _durabilityTransform = gameObject.FindChildWithName<RectTransform>("Max");
            _nameText = gameObject.FindChildWithName<EnhancedText>("Name");
            _durabilityParticles = gameObject.FindChildWithName<ParticleSystem>("Current");
            _damageText = gameObject.FindChildWithName<EnhancedText>("Damage");
            _fireRateText = gameObject.FindChildWithName<EnhancedText>("Fire Rate");
            _dpsText = gameObject.FindChildWithName<EnhancedText>("DPS");
            _capacityText = gameObject.FindChildWithName<EnhancedText>("Capacity");
            _reloadSpeedText = gameObject.FindChildWithName<EnhancedText>("Reload Speed");
            _accuracyText = gameObject.FindChildWithName<EnhancedText>("Critical Chance");
            _handlingText = gameObject.FindChildWithName<EnhancedText>("Handling");
            _inscriptionText = gameObject.FindChildWithName<EnhancedText>("Inscription");

            _inscribeButton = gameObject.FindChildWithName<EnhancedButton>("Inscribe");
            _swapButton = gameObject.FindChildWithName<EnhancedButton>("Swap");
            _infuseButton = gameObject.FindChildWithName<EnhancedButton>("Infuse");
            _infuseButton.AddOnClick(Infuse);

            _swapButton = gameObject.FindChildWithName<EnhancedButton>("Info");
            _swapButton.AddOnClick(() =>
            {
                if (!GearIsAvailable()) return;
                UiGearMenuController.EnableInput();
                _showWeapons = true;
                UiGearMenuController.SelectGear();
            });
            _inscribeButton.AddOnClick(() =>
            {
                if (!InscriptionsAreAvailable()) return;
                UiGearMenuController.EnableInput();
                _showWeapons = false;
                UiGearMenuController.SelectGear();
            });
        }

        private bool InscriptionsAreAvailable() => UiGearMenuController.Inventory().Inscriptions.Count != 0;

        private void Infuse()
        {
            if (CharacterManager.SelectedCharacter.EquippedWeapon == null) return;
            if (CharacterManager.SelectedCharacter.EquippedWeapon.WeaponAttributes.GetDurability().ReachedMax()) return;
            if (WorldState.HomeInventory().GetResourceQuantity("Essence") == 0) return;
            WorldState.HomeInventory().DecrementResource("Essence", 1);
            CharacterManager.SelectedCharacter.BrandManager.IncreaseEssenceInfused();
            int durabilityGain = 1 + (int)CharacterManager.SelectedCharacter.Attributes.Val(AttributeType.EssenceRecoveryBonus);
            CharacterManager.SelectedCharacter.EquippedWeapon.WeaponAttributes.IncreaseDurability(durabilityGain);
            UpdateDurabilityParticles();
        }

        public override void Show()
        {
            base.Show();
            SetWeapon();
            _swapButton.Select();
        }

        public override void StopComparing()
        {
            SetWeapon();
        }

        public override List<MyGameObject> GetAvailableGear()
        {
            return _showWeapons ? new List<MyGameObject>(UiGearMenuController.Inventory().Weapons) : new List<MyGameObject>(UiGearMenuController.Inventory().Inscriptions);
        }

        public override void Equip(int selectedGear)
        {
            if (selectedGear == -1) return;
            if (_showWeapons)
            {
                CharacterManager.SelectedCharacter.EquipWeapon(UiGearMenuController.Inventory().Weapons[selectedGear]);
            }
            else
            {
                CharacterManager.SelectedCharacter.EquippedWeapon.SetInscription(UiGearMenuController.Inventory().Inscriptions[selectedGear]);
            }

            Show();
        }

        public override Button GetGearButton() => _swapButton.Button();

        public override void CompareTo(MyGameObject comparisonItem)
        {
            if (comparisonItem == null) return;
            Weapon compareWeapon = comparisonItem as Weapon;
            if (CharacterManager.SelectedCharacter.EquippedWeapon == null)
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
            Weapon equipped = CharacterManager.SelectedCharacter.EquippedWeapon;
            float equippedValue = equipped.GetAttributeValue(attribute);
            if (attribute == AttributeType.Capacity)
            {
                equippedValue = Mathf.FloorToInt(equippedValue);
            }

            string prefixString = equippedValue.Round(1).ToString();
            if (compare == null) return prefixString;
            float compareValue = compare.GetAttributeValue(attribute);
            if (attribute == AttributeType.Capacity)
            {
                compareValue = Mathf.FloorToInt(compareValue);
            }

            prefixString = "<color=#505050>" + compareValue.Round(1) + "</color>" + " vs " + prefixString;
            return prefixString;
        }

        private void UpdateDurabilityParticles()
        {
            float absoluteMaxDurability = ((int) ItemQuality.Radiant + 1) * 10;
            float maxDurability = ((int) CharacterManager.SelectedCharacter.EquippedWeapon.Quality() + 1) * 10;
            float currentDurability = CharacterManager.SelectedCharacter.EquippedWeapon.WeaponAttributes.GetDurability().CurrentValue();
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
            _damageText.Text(attr.Val(AttributeType.Damage).Round(1) + " Dam");
            _fireRateText.Text(attr.Val(AttributeType.FireRate).Round(1) + " RoF");
            _reloadSpeedText.Text(attr.Val(AttributeType.ReloadSpeed).Round(1) + "s Reload");
            _handlingText.Text(attr.Val(AttributeType.Handling).Round(1) + "% Handling");
            _dpsText.Text(attr.DPS().Round(1) + " DPS");
            _capacityText.Text(attr.Val(AttributeType.Capacity).Round(1) + " Capacity");
        }

        private void SetTopToBottomNavigation(EnhancedButton button)
        {
            button.SetDownNavigation(UiGearMenuController.GetCloseButton());
        }

        private void SetNavigation()
        {
            bool infuseActive = _infuseButton.Button().interactable;
            SetTopToBottomNavigation(infuseActive ? _infuseButton : _inscribeButton);
            _inscribeButton.SetDownNavigation(UiGearMenuController.GetCloseButton(), false);
        }

        private void SetWeapon()
        {
            Weapon weapon = CharacterManager.SelectedCharacter.EquippedWeapon;
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

        public override bool GearIsAvailable() => UiGearMenuController.Inventory().Weapons.Count != 0;

        public override void SelectGearItem(MyGameObject item, UiGearMenuController.GearUi gearUi)
        {
            Weapon weapon = item as Weapon;
            if (weapon != null)
            {
                Assert.IsTrue(_showWeapons);
                gearUi.SetTypeText(weapon.GetWeaponType());
                gearUi.SetNameText(weapon.Name);
                gearUi.SetDpsText(weapon.WeaponAttributes.DPS().Round(1) + " DPS");
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