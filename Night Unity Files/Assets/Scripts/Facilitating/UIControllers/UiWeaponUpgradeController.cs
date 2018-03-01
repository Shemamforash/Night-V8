using System.Collections.Generic;
using Facilitating.UI.Elements;
using Game.Characters.Player;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UiWeaponUpgradeController : UiGearMenuTemplate
    {
        private bool _upgradingAllowed;

        private EnhancedText _typeText, _nameText, _durabilityText;
        private EnhancedText _damageText, _fireRateText, _rangeText;
        private EnhancedText _dpsText, _capacityText;
        private EnhancedText _reloadSpeedText, _criticalText, _handlingText;
        private EnhancedText _inscriptionText;

        private EnhancedButton _inscribeButton, _repairButton;
        private EnhancedButton _weaponButton;

        public void Awake()
        {
            _typeText = Helper.FindChildWithName<EnhancedText>(gameObject, "Type");
            _nameText = Helper.FindChildWithName<EnhancedText>(gameObject, "Name");
            _durabilityText = Helper.FindChildWithName<EnhancedText>(gameObject, "Durability");
            _damageText = Helper.FindChildWithName<EnhancedText>(gameObject, "Damage");
            _fireRateText = Helper.FindChildWithName<EnhancedText>(gameObject, "Fire Rate");
            _rangeText = Helper.FindChildWithName<EnhancedText>(gameObject, "Range");
            _dpsText = Helper.FindChildWithName<EnhancedText>(gameObject, "DPS");
            _capacityText = Helper.FindChildWithName<EnhancedText>(gameObject, "Capacity");
            _reloadSpeedText = Helper.FindChildWithName<EnhancedText>(gameObject, "Reload Speed");
            _criticalText = Helper.FindChildWithName<EnhancedText>(gameObject, "Critical Chance");
            _handlingText = Helper.FindChildWithName<EnhancedText>(gameObject, "Handling");
            _inscriptionText = Helper.FindChildWithName<EnhancedText>(gameObject, "Inscription");

            _inscribeButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Inscribe");
            _repairButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Repair");

            _weaponButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Info");
            _weaponButton.AddOnClick(() =>
            {
                if (GearIsAvailable()) UiGearMenuController.EnableInput();
            });
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

        public override List<GearItem> GetAvailableGear()
        {
            return new List<GearItem>(WorldState.HomeInventory().Weapons());
        }

        public override void Equip(int selectedGear)
        {
            if (selectedGear == -1) return;
            CurrentPlayer.EquipWeapon(WorldState.HomeInventory().Weapons()[selectedGear]);
            Show(CurrentPlayer);
        }

        public override Button GetGearButton()
        {
            return _weaponButton.Button();
        }

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
                _rangeText.Text(GetAttributePrefix(compareWeapon, AttributeType.Range) + "M");
                _reloadSpeedText.Text(GetAttributePrefix(compareWeapon, AttributeType.ReloadSpeed) + "s Reload ");
                _criticalText.Text(GetAttributePrefix(compareWeapon, AttributeType.CriticalChance) + "% Critical ");
                _handlingText.Text(GetAttributePrefix(compareWeapon, AttributeType.Handling) + "% Handling ");
                _capacityText.Text(GetAttributePrefix(compareWeapon, AttributeType.Capacity) + " Capacity");
            }
        }

        private string GetAttributePrefix(Weapon compare, AttributeType attribute)
        {
            Weapon equipped = CurrentPlayer.Weapon;
            float equippedValue = equipped.GetAttributeValue(attribute);
            float compareValue = compare.GetAttributeValue(attribute);
            return "<color=#505050>" + Helper.Round(compareValue, 1) + "</color>" + " vs " + Helper.Round(equippedValue, 1);
        }

        private void SetWeaponInfo(Weapon weapon)
        {
            WeaponAttributes attr = weapon.WeaponAttributes;
            _typeText.Text(weapon.GetWeaponType());
            _nameText.Text(weapon.ExtendedName());
            _durabilityText.Text(attr.Durability.CurrentValue() + " Durability");
            _damageText.Text(Helper.Round(attr.Damage.CurrentValue(), 1) + " Dam");
            _fireRateText.Text(Helper.Round(attr.FireRate.CurrentValue(), 1) + " RoF");
            _rangeText.Text(Helper.Round(attr.Range.CurrentValue(), 1) + "M");
            _reloadSpeedText.Text(Helper.Round(attr.ReloadSpeed.CurrentValue(), 1) + "s Reload");
            _criticalText.Text(Helper.Round(attr.CriticalChance.CurrentValue(), 1) + "% Critical");
            _handlingText.Text(Helper.Round(attr.Handling.CurrentValue(), 1) + "% Handling");
            _dpsText.Text(Helper.Round(attr.DPS(), 1) + " DPS");
            _capacityText.Text(Helper.Round(attr.Capacity.CurrentValue(), 1) + " Capacity");
        }

        private void SetTopToBottomNavigation(EnhancedButton button)
        {
            _weaponButton.SetDownNavigation(button);
            button.SetDownNavigation(UiGearMenuController._closeButton);
        }

        private void SetNavigation()
        {
            bool inscribeActive = _inscribeButton.Button().interactable;
            bool repairActive = _repairButton.Button().interactable;

            if (repairActive) SetTopToBottomNavigation(_repairButton);
            else if (inscribeActive)
                SetTopToBottomNavigation(_inscribeButton);
            else
                _weaponButton.SetDownNavigation(UiGearMenuController._closeButton);

            if (inscribeActive)
            {
                _inscribeButton.SetUpNavigation(_weaponButton, false);
                _inscribeButton.SetDownNavigation(UiGearMenuController._closeButton, false);
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
            _repairButton.Button().interactable = !attr.Durability.ReachedMax();

            SetNavigation();
        }

        private void SetNoWeaponInfo()
        {
            _typeText.Text("");
            _nameText.Text("");
            _durabilityText.Text("");
            _damageText.Text("");
            _fireRateText.Text("");
            _rangeText.Text("");
            _dpsText.Text("Nothing Equipped");
            _capacityText.Text("");
            _reloadSpeedText.Text("");
            _criticalText.Text("");
            _handlingText.Text("");
            _inscriptionText.Text("");
        }

        private void SetNoWeapon()
        {
            SetNoWeaponInfo();
            _inscribeButton.Button().interactable = false;
            _repairButton.Button().interactable = false;
            SetNavigation();
        }

        public override bool GearIsAvailable()
        {
            return WorldState.HomeInventory().Weapons().Count != 0;
        }

        public override void SelectGearItem(GearItem item, UiGearMenuController.GearUi gearUi)
        {
            Weapon weapon = item as Weapon;
            gearUi.SetTypeText(weapon.GetWeaponType());
            gearUi.SetNameText(weapon.ExtendedName());
            gearUi.SetDpsText(Helper.Round(weapon.WeaponAttributes.DPS(), 1) + " DPS");
        }
    }
}