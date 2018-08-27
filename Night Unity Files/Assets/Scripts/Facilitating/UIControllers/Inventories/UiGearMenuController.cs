using System;
using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Misc;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
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
        private readonly List<Action> _tabs = new List<Action>();
        private static int _currentTab;
        private static Player _currentPlayer;

        private static UiAccessoryController _accessoryController;
        private static UiArmourUpgradeController _armourUpgradeController;
        private static UiWeaponUpgradeController _weaponUpgradeController;
        private static UICraftingController _craftingController;
        private static UiConsumableController _consumableController;
        private static UiJournalController _journalController;
        private static UiGearMenuTemplate _currentInventoryMenu;
        private static EnhancedButton _closeButton;
        private static EnhancedButton _centreButton;
        private static bool _open;

        private static readonly List<GearUi> _itemUiList = new List<GearUi>();
        private static int _selectedItem;
        private static bool _menuListInteractable;
        private static Inventory _currentInventory;
        [SerializeField]
        private bool HideCraftingMenu;
        
        public static EnhancedButton GetCentreButton()
        {
            return _centreButton;
        }

        public static Inventory Inventory()
        {
            return _currentInventory;
        }
        
        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (isHeld) return;
            switch (axis)
            {
                case InputAxis.SwitchTab:
                    if (direction < 0 && _currentTab - 1 >= 0)
                    {
                        SetCurrentTab(_currentTab - 1);
                    }
                    else if (direction > 0 && _currentTab + 1 < _tabs.Count)
                    {
                        SetCurrentTab(_currentTab + 1);
                    }

                    break;
                case InputAxis.Cover:
                    Equip(-1);
                    break;
                case InputAxis.Vertical:
                    if (!_menuListInteractable) return;
                    if (direction < 0)
                    {
                        TrySelectGearBelow();
                    }
                    else
                    {
                        TrySelectGearAbove();
                    }

                    break;
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        private void CreateTab(Action<Player> buttonAction)
        {
            _tabs.Add(() => buttonAction(_currentPlayer));
        }

        public override void Awake()
        {
            base.Awake();
            _tabs.Clear();
            CreateTab(ShowWeaponMenu);
            CreateTab(ShowArmourMenu);
            CreateTab(ShowAccessoryMenu);
            CreateTab(ShowConsumableMenu);
            if (!HideCraftingMenu)
            {
                CreateTab(ShowCraftingMenu);
            }
            else
            {
                gameObject.FindChildWithName("Tabs").FindChildWithName("Crafting").SetActive(false);
            }

            GameObject gearObject = gameObject.FindChildWithName("Gear");
            _accessoryController = gearObject.FindChildWithName<UiAccessoryController>("Accessory");
            _armourUpgradeController = gearObject.FindChildWithName<UiArmourUpgradeController>("Armour");
            _weaponUpgradeController = gearObject.FindChildWithName<UiWeaponUpgradeController>("Weapon");
            _craftingController = gearObject.FindChildWithName<UICraftingController>("Crafting");
            _consumableController = gearObject.FindChildWithName<UiConsumableController>("Consumables");
            _journalController = gearObject.FindChildWithName<UiJournalController>("Journals");
            
            _closeButton = gameObject.FindChildWithName<EnhancedButton>("Close");
            _closeButton.AddOnClick(Close);

            InitialiseGearList();
        }

        private void Close()
        {
            _armourUpgradeController.Hide();
            _weaponUpgradeController.Hide();
            _accessoryController.Hide();
            MenuStateMachine.ReturnToDefault();
            _open = false;
        }

        private void SetCurrentTab(int currentTab)
        {
            if (currentTab == 4 && HideCraftingMenu) return;
            _currentTab = currentTab;
            _tabs[_currentTab]();
        }

        public override void Enter()
        {
            base.Enter();
            InputHandler.SetCurrentListener(this);
        }

        private static void OpenInventoryMenu(Player player, int tabNumber, UiGearMenuTemplate gearMenu)
        {
            _currentInventory = player.TravelAction.AtHome() ? WorldState.HomeInventory() : player.Inventory();
            _menuListInteractable = false;
            _selectedItem = 0;
            _currentPlayer = player;
            if (!_open)
            {
                MenuStateMachine.ShowMenu("Inventories");
                _open = true;
            }

            _currentInventoryMenu = gearMenu;
            TrySelectMenu(_armourUpgradeController);
            TrySelectMenu(_accessoryController);
            TrySelectMenu(_consumableController);
            TrySelectMenu(_craftingController);
            TrySelectMenu(_journalController);
            _currentTab = tabNumber;
        }

        private static void TrySelectMenu(UiGearMenuTemplate gearMenu)
        {
            if (_currentInventoryMenu == gearMenu)
                gearMenu.Show();
            else
                gearMenu.Hide();
        }

        public static void ShowArmourMenu(Player player)
        {
            OpenInventoryMenu(player, 1, _armourUpgradeController);
        }

        public static void ShowWeaponMenu(Player player)
        {
            OpenInventoryMenu(player, 0, null);
            _weaponUpgradeController.Show();
        }

        public static void ShowAccessoryMenu(Player player)
        {
            OpenInventoryMenu(player, 2, _accessoryController);
        }

        public static void ShowCraftingMenu(Player player)
        {
            OpenInventoryMenu(player, 4, _craftingController);
        }

        public static void ShowConsumableMenu(Player player)
        {
            OpenInventoryMenu(player, 3, _consumableController);
        }

        private void Equip(int gearIndex)
        {
            SelectGear();
            DisableInput();
            _currentInventoryMenu.Equip(gearIndex);
            _currentInventoryMenu.GetGearButton().Select();
        }

        private void InitialiseGearList()
        {
            _itemUiList.Clear();
            GameObject gearListObject = gameObject.FindChildWithName("Gear List");
            for (int i = 0; i < 7; ++i)
            {
                GameObject uiObject = gearListObject.FindChildWithName("Item " + i);
                GearUi gearUi = new GearUi(uiObject, Math.Abs(i - centre));

                if (i == centre) _centreButton = uiObject.GetComponent<EnhancedButton>();

                _itemUiList.Add(gearUi);
                gearUi.SetGear(null);
            }

            _centreButton.AddOnClick(() => Equip(_selectedItem));
        }

        public static void EnableInput()
        {
            SelectGear();
            _centreButton.Select();
            _menuListInteractable = true;
        }

        private void DisableInput()
        {
            _menuListInteractable = false;
            _currentInventoryMenu.StopComparing();
            _selectedItem = 0;
        }

        private void TrySelectGearBelow()
        {
            if (_selectedItem == _currentInventoryMenu.GetAvailableGear().Count - 1) return;
            ++_selectedItem;
            SelectGear();
        }

        private void TrySelectGearAbove()
        {
            if (_selectedItem == 0) return;
            --_selectedItem;
            SelectGear();
        }

        public static void SelectGear()
        {
            for (int i = 0; i < _itemUiList.Count; ++i)
            {
                int offset = i - centre;
                int targetGear = _selectedItem + offset;
                MyGameObject gearItem = null;
                if (targetGear >= 0 && targetGear < _currentInventoryMenu.GetAvailableGear().Count)
                {
                    gearItem = _currentInventoryMenu.GetAvailableGear()[targetGear];
                }

                if (i == centre && gearItem != null) _currentInventoryMenu.CompareTo(gearItem);

                _itemUiList[i].SetGear(gearItem);
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
                _typeText = uiObject.FindChildWithName<EnhancedText>("Type");
                _nameText = uiObject.FindChildWithName<EnhancedText>("Name");
                _dpsText = uiObject.FindChildWithName<EnhancedText>("Dps");
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
                _dpsText.SetText(text);
            }

            public void SetNameText(string text)
            {
                _nameText.SetColor(text == "" ? UiAppearanceController.InvisibleColour : _activeColour);
                _nameText.SetText(text);
            }

            public void SetTypeText(string text)
            {
                _typeText.SetColor(text == "" ? UiAppearanceController.InvisibleColour : _activeColour);
                _typeText.SetText(text);
            }

            public void SetGear(MyGameObject gearItem)
            {
                if (gearItem == null)
                {
                    SetColour(UiAppearanceController.InvisibleColour);
                    return;
                }

                _currentInventoryMenu.SelectGearItem(gearItem, this);
            }
        }

        public static EnhancedButton GetCloseButton()
        {
            return _closeButton;
        }
    }
}