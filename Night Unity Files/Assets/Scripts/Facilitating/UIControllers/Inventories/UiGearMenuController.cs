using System;
using System.Collections.Generic;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Global;
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
        private static UiAccessoryController _accessoryController;
        private static UiArmourUpgradeController _armourUpgradeController;
        private static UiWeaponUpgradeController _weaponUpgradeController;
        private static UICraftingController _craftingController;
        private static UiConsumableController _consumableController;
        private static UiJournalController _journalController;
        private static UiInventoryMenuController _currentMenuController;
        private static UiGearMenuController _instance;

        private static Inventory _currentInventory;
        private static Tab _currentTab;
        private static bool _open;

        private readonly List<Tab> _tabs = new List<Tab>();

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
                    if(direction < 0) _currentTab.SelectPreviousTab();
                    else _currentTab.SelectNextTab();
                    break;
                case InputAxis.Cover:
//                    Close();
                    break;
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        private void CreateTab(string tabName, Action onSelectAction)
        {
            Tab tab = new Tab(tabName, gameObject.FindChildWithName("Tabs"), onSelectAction);
            _tabs.Add(tab);
        }

        private class Tab
        {
            private readonly EnhancedText _tabText;
            private readonly Action OnSelectAction;
            private Tab _prevTab, _nextTab;

            public Tab(string name, GameObject parent, Action onSelectAction)
            {
                _tabText = parent.FindChildWithName<EnhancedText>(name);
                OnSelectAction = onSelectAction;
            }

            private void Select()
            {
                _currentTab = this;
                Debug.Log("waffle");
                OnSelectAction();
                _tabText.SetUnderlineActive(true);
            }

            private void Deselect()
            {
                _tabText.SetUnderlineActive(false);
            }

            public void SetNeighbors(Tab prevTab, Tab nextTab)
            {
                _prevTab = prevTab;
                _nextTab = nextTab;
            }

            public void SelectNextTab()
            {
                if (_nextTab == null) return;
                Deselect();
                _nextTab.Select();
            }

            public void SelectPreviousTab()
            {
                if (_prevTab == null) return;
                Deselect();
                _prevTab.Select();
            }
        }

        public override void Awake()
        {
            base.Awake();
            _instance = this;
            _tabs.Clear();
            CreateTab("Weapons", ShowWeaponMenu);
            CreateTab("Armour", ShowArmourMenu);
            CreateTab("Accessories", ShowAccessoryMenu);
            CreateTab("Crafting", ShowCraftingMenu);
            CreateTab("Consumables", ShowConsumableMenu);
            CreateTab("Journals", ShowJournalMenu);
            for (int i = 0; i < _tabs.Count; ++i)
            {
                int prevIndex = i - 1;
                Tab prevTab = prevIndex == -1 ? null : _tabs[prevIndex];
                
                int nextIndex = i + 1;
                Tab nextTab = nextIndex == _tabs.Count ? null : _tabs[nextIndex];
                
                _tabs[i].SetNeighbors(prevTab, nextTab);
            }
            GameObject gearObject = gameObject.FindChildWithName("Gear");
            _accessoryController = gearObject.FindChildWithName<UiAccessoryController>("Accessory");
            _armourUpgradeController = gearObject.FindChildWithName<UiArmourUpgradeController>("Armour");
            _weaponUpgradeController = gearObject.FindChildWithName<UiWeaponUpgradeController>("Weapon");
            _craftingController = gearObject.FindChildWithName<UICraftingController>("Crafting");
            _consumableController = gearObject.FindChildWithName<UiConsumableController>("Consumables");
            _journalController = gearObject.FindChildWithName<UiJournalController>("Journals");
        }

        public static void Close()
        {
            _currentMenuController.Hide();
            InputHandler.UnregisterInputListener(_instance);
            MenuStateMachine.ReturnToDefault();
            _open = false;
        }

        public override void Enter()
        {
            base.Enter();
            InputHandler.RegisterInputListener(this);
            _currentTab = _tabs[0];
        }

        private static void OpenInventoryMenu(UiInventoryMenuController menuController)
        {
            Player player = CharacterManager.SelectedCharacter;
            _currentInventory = player.TravelAction.AtHome() ? WorldState.HomeInventory() : player.Inventory();
            if (_currentMenuController != null) _currentMenuController.Hide();
            _currentMenuController = menuController;
            if (!_open)
            {
                MenuStateMachine.ShowMenu("Inventories", _currentMenuController.Show);
                _open = true;
            }
            else
            {
                _currentMenuController.Show();
            }
        }

        public static void ShowArmourMenu() => OpenInventoryMenu(_armourUpgradeController);

        public static void ShowWeaponMenu() => OpenInventoryMenu(_weaponUpgradeController);

        public static void ShowAccessoryMenu() => OpenInventoryMenu(_accessoryController);

        public static void ShowCraftingMenu() => OpenInventoryMenu(_craftingController);

        public static void ShowConsumableMenu() => OpenInventoryMenu(_consumableController);

        public static void ShowJournalMenu() => OpenInventoryMenu(_journalController);
    }
}