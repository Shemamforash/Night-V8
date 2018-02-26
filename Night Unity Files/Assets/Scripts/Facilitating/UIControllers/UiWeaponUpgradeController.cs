using System;
using System.Collections.Generic;
using Facilitating.UI.Elements;
using Game.Characters.Player;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UiWeaponUpgradeController : Menu, IInputListener
    {
        private static string _previousMenu;
        private static bool _upgradingAllowed;

        private static GameObject _weaponInfoObject;
        private static GameObject _weaponCompareObject;

        private static EnhancedText _typeText, _nameText, _durabilityText;
        private static EnhancedText _damageText, _fireRateText, _rangeText;
        private static EnhancedText _dpsText, _capacityText;
        private static EnhancedText _reloadSpeedText, _criticalText, _handlingText;
        private static EnhancedText _inscriptionText;

        private static EnhancedButton _inscribeButton, _repairButton, _compareButton, _equipButton;
        private static EnhancedButton _centreButton;

        private static Player _currentPlayer;

        private static readonly List<WeaponUI> _weaponUis = new List<WeaponUI>();
        private static int _selectedWeapon;

        public void Awake()
        {
            _weaponInfoObject = Helper.FindChildWithName(gameObject, "Weapon Info");
            _weaponCompareObject = Helper.FindChildWithName(gameObject, "Weapon List");

            _typeText = Helper.FindChildWithName<EnhancedText>(_weaponInfoObject, "Type");
            _nameText = Helper.FindChildWithName<EnhancedText>(_weaponInfoObject, "Name");
            _durabilityText = Helper.FindChildWithName<EnhancedText>(_weaponInfoObject, "Durability");
            _damageText = Helper.FindChildWithName<EnhancedText>(_weaponInfoObject, "Damage");
            _fireRateText = Helper.FindChildWithName<EnhancedText>(_weaponInfoObject, "Fire Rate");
            _rangeText = Helper.FindChildWithName<EnhancedText>(_weaponInfoObject, "Range");
            _dpsText = Helper.FindChildWithName<EnhancedText>(_weaponInfoObject, "DPS");
            _capacityText = Helper.FindChildWithName<EnhancedText>(_weaponInfoObject, "Capacity");
            _reloadSpeedText = Helper.FindChildWithName<EnhancedText>(_weaponInfoObject, "Reload Speed");
            _criticalText = Helper.FindChildWithName<EnhancedText>(_weaponInfoObject, "Critical Chance");
            _handlingText = Helper.FindChildWithName<EnhancedText>(_weaponInfoObject, "Handling");
            _inscriptionText = Helper.FindChildWithName<EnhancedText>(_weaponInfoObject, "Inscription");

            _inscribeButton = Helper.FindChildWithName<EnhancedButton>(_weaponInfoObject, "Inscribe");
            _repairButton = Helper.FindChildWithName<EnhancedButton>(_weaponInfoObject, "Repair");
            _compareButton = Helper.FindChildWithName<EnhancedButton>(_weaponInfoObject, "Compare");
            _equipButton = Helper.FindChildWithName<EnhancedButton>(_weaponInfoObject, "Equip");

            for (int i = 0; i < 7; ++i)
            {
                GameObject uiObject = Helper.FindChildWithName(_weaponCompareObject, "Item " + i);
                WeaponUI weaponUi = new WeaponUI(uiObject, Math.Abs(i - 3));
                if (i == 3)
                {
                    _centreButton = uiObject.GetComponent<EnhancedButton>();
                }

                _weaponUis.Add(weaponUi);
                weaponUi.SetWeapon(null);
            }

            _centreButton.AddOnClick(() =>
            {
                Equip(WorldState.HomeInventory().Weapons()[_selectedWeapon]);
                DisableInput();
            });

            _equipButton.AddOnClick(EnableInput);
            _compareButton.AddOnClick(EnableInput);
        }

        private void ReturnToPreviousMenu()
        {
            MenuStateMachine.ShowMenu(_previousMenu);
        }

        public static void Show(Player player)
        {
            _previousMenu = MenuStateMachine.States.GetCurrentState().Name;
            MenuStateMachine.ShowMenu("Weapon Upgrade Menu");
            _currentPlayer = player;
            if (player.Weapon != null)
            {
                SetWeapon();
            }
            else
            {
                SetNoWeapon();
            }
        }

        public void Equip(Weapon weapon)
        {
            if (weapon != null)
            {
                _currentPlayer.EquipWeapon(weapon);
                Show(_currentPlayer);
                return;
            }

            SetNoWeaponInfo();
        }

        public void CompareTo(Weapon weapon)
        {
            if (_currentPlayer.Weapon == null)
            {
                SetWeaponInfo(weapon);
            }
            else
            {
                WeaponAttributes attr = _currentPlayer.Weapon.WeaponAttributes;
                _damageText.Text(GetAttributePrefix(weapon, AttributeType.Damage) + " " + Helper.Round(attr.Damage.CurrentValue(), 1) + " Dam");
                _fireRateText.Text(GetAttributePrefix(weapon, AttributeType.FireRate) + " " + Helper.Round(attr.FireRate.CurrentValue(), 1) + " RoF");
                _rangeText.Text(GetAttributePrefix(weapon, AttributeType.Range) + " " + Helper.Round(attr.Range.CurrentValue(), 1) + "M");
                _reloadSpeedText.Text(Helper.Round(attr.ReloadSpeed.CurrentValue(), 1) + "s Reload " + GetAttributePrefix(weapon, AttributeType.ReloadSpeed));
                _criticalText.Text(Helper.Round(attr.CriticalChance.CurrentValue(), 1) + "% Critical " + GetAttributePrefix(weapon, AttributeType.CriticalChance));
                _handlingText.Text(Helper.Round(attr.Handling.CurrentValue(), 1) + "% Handling " + GetAttributePrefix(weapon, AttributeType.Handling));
                _dpsText.Text(GetPrefix(attr.DPS(), weapon.WeaponAttributes.DPS()) + " " + Helper.Round(attr.DPS(), 1) + " DPS");
                _capacityText.Text(GetAttributePrefix(weapon, AttributeType.Capacity) + " " + Helper.Round(attr.Capacity.CurrentValue(), 1) + " Capacity");
            }
        }

        public void StopComparing()
        {
            if (_currentPlayer.Weapon == null)
            {
                SetNoWeaponInfo();
            }
            else
            {
                SetWeaponInfo(_currentPlayer.Weapon);
            }
        }

        private static string GetPrefix(float original, float other)
        {
            string prefix = "";

            if (other > original)
            {
                prefix += "-";
                if (other > original * 1.2f)
                {
                    prefix += " -";
                }
            }
            else
            {
                prefix += "+";
                if (other < original * 0.8f)
                {
                    prefix += " +";
                }
            }

            return prefix;
        }

        private string GetAttributePrefix(Weapon compare, AttributeType attribute)
        {
            Weapon equipped = _currentPlayer.Weapon;
            float equippedValue = equipped.GetAttributeValue(attribute);
            float compareValue = compare.GetAttributeValue(attribute);
            return GetPrefix(equippedValue, compareValue);
        }

        private static void SetWeaponInfo(Weapon weapon)
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

        private static void SetWeapon()
        {
            Weapon weapon = _currentPlayer.Weapon;
            WeaponAttributes attr = weapon.WeaponAttributes;
            SetWeaponInfo(weapon);
            _compareButton.gameObject.SetActive(true);
            _equipButton.gameObject.SetActive(false);
            _compareButton.Button().Select();
//        if (weapon.Inscribable())
//        {
//            _inscribeButton.gameObject.SetActive(true);
//            _inscriptionText.Text("-- Not Inscribable --");
//        }
//        else
//        {
            //todo show inscription
//            weapon.Inscription.Name
//            _inscriptionText.Text("-- Not Inscribable --");
//        }

            if (attr.Durability.ReachedMax())
            {
                _repairButton.gameObject.SetActive(false);
            }
            else
            {
                _repairButton.gameObject.SetActive(true);
            }

            if (!WeaponsAreAvailable())
            {
                _compareButton.gameObject.SetActive(false);
            }
        }

        private static void SetNoWeaponInfo()
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

        private static void SetNoWeapon()
        {
            SetNoWeaponInfo();
            _inscribeButton.gameObject.SetActive(false);
            _repairButton.gameObject.SetActive(false);
            _compareButton.gameObject.SetActive(false);
            _equipButton.gameObject.SetActive(true);
            _equipButton.Button().Select();
            if (!WeaponsAreAvailable())
            {
                _equipButton.gameObject.SetActive(false);
            }
        }

        public static bool WeaponsAreAvailable()
        {
            return WorldState.HomeInventory().Weapons().Count != 0;
        }

        public void EnableInput()
        {
            InputHandler.RegisterInputListener(this);
            _centreButton.Button().Select();
        }

        private void DisableInput()
        {
            InputHandler.UnregisterInputListener(this);
            StopComparing();
        }

        private void SelectWeapon()
        {
            int centre = 3;
            for (int i = 0; i < _weaponUis.Count; ++i)
            {
                int offset = i - centre;
                int targetWeapon = _selectedWeapon + offset;
                Weapon weapon = null;
                if (targetWeapon >= 0 && targetWeapon < WorldState.HomeInventory().Weapons().Count)
                {
                    weapon = WorldState.HomeInventory().Weapons()[targetWeapon];
                }

                if (i == centre)
                {
                    CompareTo(weapon);
                }

                _weaponUis[i].SetWeapon(weapon);
            }
        }

        private void TrySelectWeaponBelow()
        {
            if (_selectedWeapon == WorldState.HomeInventory().Weapons().Count - 1) return;
            ++_selectedWeapon;
            SelectWeapon();
        }

        private void TrySelectWeaponAbove()
        {
            if (_selectedWeapon == 0) return;
            --_selectedWeapon;
            SelectWeapon();
        }

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (isHeld) return;
            if (axis == InputAxis.Cover)
            {
                DisableInput();
                Equip(null);
            }
            else if (axis != InputAxis.Vertical)
                return;

            if (direction < 0)
            {
                TrySelectWeaponBelow();
            }
            else
            {
                TrySelectWeaponAbove();
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        private class WeaponUI
        {
            private readonly EnhancedText _typeText, _nameText, _dpsText;
            private readonly Color _activeColour;

            public WeaponUI(GameObject uiObject, int offset)
            {
                _typeText = Helper.FindChildWithName<EnhancedText>(uiObject, "Type");
                _nameText = Helper.FindChildWithName<EnhancedText>(uiObject, "Name");
                _dpsText = Helper.FindChildWithName<EnhancedText>(uiObject, "Dps");
                _activeColour = new Color(1f, 1f, 1f, 1f / (offset + 1));
            }

            private void SetColour(Color c)
            {
                _typeText.SetColor(c);
                _nameText.SetColor(c);
                _dpsText.SetColor(c);
            }

            public void SetWeapon(Weapon weapon)
            {
                if (weapon == null)
                {
                    SetColour(new Color(1, 1, 1, 0f));
                    return;
                }

                SetColour(_activeColour);
                _typeText.Text(weapon.GetWeaponType());
                _nameText.Text(weapon.ExtendedName());
                _dpsText.Text(Helper.Round(weapon.WeaponAttributes.DPS(), 1) + " DPS");
            }
        }
    }
}