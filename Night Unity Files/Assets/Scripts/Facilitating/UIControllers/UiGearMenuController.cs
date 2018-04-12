using System;
using System.Collections.Generic;
using Game.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

namespace Facilitating.UIControllers
{
    public class UiGearMenuController : Menu, IInputListener
    {
        private const int centre = 3;
        private readonly List<EnhancedButton> _tabs = new List<EnhancedButton>();
        private int _currentTab;
        private Player _currentPlayer;

        private UiGearMenuTemplate _accessoryController;
        private UiGearMenuTemplate _armourUpgradeController;
        private UiGearMenuTemplate _weaponUpgradeController;
        private UiGearMenuTemplate _currentGearMenu;
        public EnhancedButton _closeButton;
        private static UiGearMenuController _instance;
        private bool _open;

        private EnhancedButton _centreButton;
        private readonly List<GearUi> _gearUis = new List<GearUi>();
        private int _selectedGear;
        private bool _gearSelectAllowed;

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (isHeld) return;
            switch (axis)
            {
                case InputAxis.SwitchTab:
                    if (direction < 0 && _currentTab - 1 >= 0)
                        SetCurrentTab(_currentTab - 1);
                    else if (direction > 0 && _currentTab + 1 < _tabs.Count)
                        SetCurrentTab(_currentTab + 1);

                    break;
                case InputAxis.Cover:
                    Equip(-1);
                    break;
                case InputAxis.Vertical:
                    if (!_gearSelectAllowed) return;
                    if (direction < 0)
                        TrySelectGearBelow();
                    else
                        TrySelectGearAbove();

                    break;
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            GameObject tabObject = Helper.FindChildWithName(gameObject, "Tabs");
            EnhancedButton weaponTab = Helper.FindChildWithName<EnhancedButton>(tabObject, "Weapons");
            weaponTab.AddOnSelectEvent(() => ShowWeaponMenu(_currentPlayer));
            EnhancedButton armourTab = Helper.FindChildWithName<EnhancedButton>(tabObject, "Armour");
            armourTab.AddOnSelectEvent(() => ShowArmourMenu(_currentPlayer));
            EnhancedButton accessoryTab = Helper.FindChildWithName<EnhancedButton>(tabObject, "Accessories");
            accessoryTab.AddOnSelectEvent(() => ShowAccessoryMenu(_currentPlayer));
            _tabs.Add(weaponTab);
            _tabs.Add(armourTab);
            _tabs.Add(accessoryTab);

            GameObject gearObject = Helper.FindChildWithName(gameObject, "Gear");
            _accessoryController = Helper.FindChildWithName<UiAccessoryController>(gearObject, "Accessory");
            _armourUpgradeController = Helper.FindChildWithName<UiArmourUpgradeController>(gearObject, "Armour");
            _weaponUpgradeController = Helper.FindChildWithName<UiWeaponUpgradeController>(gearObject, "Weapon");

            _closeButton = Helper.FindChildWithName<EnhancedButton>(gameObject, "Close");
            _closeButton.AddOnClick(Close);

            InitialiseGearList();
        }

        private void Close()
        {
            _armourUpgradeController.Hide();
            _weaponUpgradeController.Hide();
            _accessoryController.Hide();
            MenuStateMachine.GoToInitialMenu();
            _open = false;
        }

        private void SetCurrentTab(int currentTab)
        {
            _currentTab = currentTab;
            _tabs[_currentTab].Button().Select();
        }

        private void OpenGearMenu(Player player, int tabNumber, UiGearMenuTemplate gearMenu)
        {
            _gearSelectAllowed = false;
            _selectedGear = 0;
            _currentPlayer = player;
            if (!_open)
            {
                MenuStateMachine.ShowMenu("Gear Menus");
                _open = true;
                InputHandler.SetCurrentListener(_instance);
            }

            _currentGearMenu = gearMenu;
            TrySelectMenu(_armourUpgradeController);
            TrySelectMenu(_accessoryController);
            TrySelectMenu(_weaponUpgradeController);
            SetCurrentTab(tabNumber);
        }

        private void TrySelectMenu(UiGearMenuTemplate gearMenu)
        {
            if (_currentGearMenu == gearMenu) gearMenu.Show(_currentPlayer);
            else gearMenu.Hide();
        }

        public void ShowArmourMenu(Player player)
        {
            OpenGearMenu(player, 1, _armourUpgradeController);
        }

        public void ShowWeaponMenu(Player player)
        {
            OpenGearMenu(player, 0, _weaponUpgradeController);
        }

        public void ShowAccessoryMenu(Player player)
        {
            OpenGearMenu(player, 2, _accessoryController);
        }

        private void Equip(int gearIndex)
        {
            SelectGear();
            DisableInput();
            _currentGearMenu.Equip(gearIndex);
            _currentGearMenu.GetGearButton().Select();
            _selectedGear = 0;
        }

        private void InitialiseGearList()
        {
            GameObject gearListObject = Helper.FindChildWithName(gameObject, "Gear List");
            for (int i = 0; i < 7; ++i)
            {
                GameObject uiObject = Helper.FindChildWithName(gearListObject, "Item " + i);
                GearUi gearUi = new GearUi(uiObject, Math.Abs(i - centre));
                if (i == centre) _centreButton = uiObject.GetComponent<EnhancedButton>();

                _gearUis.Add(gearUi);
                gearUi.SetGear(null);
            }

            _centreButton.AddOnClick(() => Equip(_selectedGear));
        }

        public void EnableInput()
        {
            SelectGear();
            _centreButton.Button().Select();
            _gearSelectAllowed = true;
        }

        private void DisableInput()
        {
            _gearSelectAllowed = false;
            _currentGearMenu.StopComparing();
        }

        private void TrySelectGearBelow()
        {
            if (_selectedGear == _currentGearMenu.GetAvailableGear().Count - 1) return;
            ++_selectedGear;
            SelectGear();
        }

        private void TrySelectGearAbove()
        {
            if (_selectedGear == 0) return;
            --_selectedGear;
            SelectGear();
        }

        public void SelectGear()
        {
            for (int i = 0; i < _gearUis.Count; ++i)
            {
                int offset = i - centre;
                int targetGear = _selectedGear + offset;
                GearItem gearItem = null;
                if (targetGear >= 0 && targetGear < _currentGearMenu.GetAvailableGear().Count) gearItem = _currentGearMenu.GetAvailableGear()[targetGear];

                if (i == centre) _currentGearMenu.CompareTo(gearItem);

                _gearUis[i].SetGear(gearItem);
            }
        }

        public class GearUi
        {
            private readonly Color _activeColour;
            private readonly EnhancedText _dpsText;
            private readonly EnhancedText _nameText;
            private readonly EnhancedText _typeText;

            public GearUi(GameObject uiObject, int offset)
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

            public void SetDpsText(string text)
            {
                _dpsText.SetColor(text == "" ? UiAppearanceController.InvisibleColour : _activeColour);
                _dpsText.Text(text);
            }

            public void SetNameText(string text)
            {
                _nameText.SetColor(text == "" ? UiAppearanceController.InvisibleColour : _activeColour);
                _nameText.Text(text);
            }

            public void SetTypeText(string text)
            {
                _typeText.SetColor(text == "" ? UiAppearanceController.InvisibleColour : _activeColour);
                _typeText.Text(text);
            }

            public void SetGear(GearItem gearItem)
            {
                if (gearItem == null)
                {
                    SetColour(UiAppearanceController.InvisibleColour);
                    return;
                }

                _instance._currentGearMenu.SelectGearItem(gearItem, this);
            }
        }

        public static UiGearMenuController Instance()
        {
            return _instance;
        }
    }
}