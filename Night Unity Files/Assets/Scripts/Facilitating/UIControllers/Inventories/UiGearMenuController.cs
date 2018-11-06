using System.Collections.Generic;
using DG.Tweening;
using Facilitating.UIControllers.Inventories;
using Game.Characters;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
        private static UiWillController _willController;
        private static UiGearMenuController _instance;
        private static CloseButtonController _closeButton;
        private static TabController _leftTab, _rightTab;

        private static Tab _currentTab;
        private static bool _open;
        private static readonly List<Tab> _tabs = new List<Tab>();
        private static GameObject _tabParent;

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (isHeld) return;
            switch (axis)
            {
                case InputAxis.SwitchTab:
                    if (direction < 0) _currentTab.SelectPreviousTab();
                    else _currentTab.SelectNextTab();
                    break;
                case InputAxis.Inventory:
                    Close();
                    break;
            }
        }

        public void OnInputUp(InputAxis axis)
        {
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
        }

        private void CreateTab(string tabName, UiInventoryMenuController menu)
        {
            Tab tab = new Tab(tabName, menu);
            _tabs.Add(tab);
        }

        private class Tab
        {
            private readonly UiInventoryMenuController _menu;
            private Tab _prevTab;
            private Tab _nextTab;
            private readonly Image _highlightImage;

            public Tab(string name, UiInventoryMenuController menu)
            {
                EnhancedText tabText = _tabParent.FindChildWithName<EnhancedText>(name);
                _highlightImage = tabText.gameObject.FindChildWithName<Image>("Image");
                _menu = menu;
            }

            public void Select()
            {
                _currentTab = this;
                _highlightImage.DOFade(0.5f, 0.5f).SetUpdate(UpdateType.Normal, true);
                OpenInventoryMenu(_menu);
            }

            public void Deselect()
            {
                _highlightImage.DOFade(0f, 0.5f).SetUpdate(UpdateType.Normal, true);
            }

            public void SetNeighbors(Tab prevTab, Tab nextTab)
            {
                _prevTab = prevTab;
                _nextTab = nextTab;
            }

            public void SelectNextTab()
            {
                _leftTab.FadeIn();
                if (_nextTab == null) return;
                if (_nextTab._nextTab == null) _rightTab.FlashAndFade();
                else _rightTab.Flash();
                Deselect();
                _nextTab.Select();
            }

            public void SelectPreviousTab()
            {
                _rightTab.FadeIn();
                if (_prevTab == null) return;
                if (_prevTab._prevTab == null) _leftTab.FlashAndFade();
                else _leftTab.Flash();
                _leftTab.Flash();
                Deselect();
                _prevTab.Select();
            }
        }

        public override void Awake()
        {
            base.Awake();
            bool isCombat = SceneManager.GetActiveScene().name == "Combat";
            
            _instance = this;

            _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");

            _tabParent = gameObject.FindChildWithName("Tabs");
            _leftTab = _tabParent.FindChildWithName<TabController>("Left Indicator");
            _rightTab = _tabParent.FindChildWithName<TabController>("Right Indicator");

            GameObject gearObject = gameObject.FindChildWithName("Gear");
            _accessoryController = gearObject.FindChildWithName<UiAccessoryController>("Accessory");
            _armourUpgradeController = gearObject.FindChildWithName<UiArmourUpgradeController>("Armour");
            _weaponUpgradeController = gearObject.FindChildWithName<UiWeaponUpgradeController>("Weapon");
            _craftingController = gearObject.FindChildWithName<UICraftingController>("Crafting");
            _consumableController = gearObject.FindChildWithName<UiConsumableController>("Consumables");
            _journalController = gearObject.FindChildWithName<UiJournalController>("Journals");
            _willController = gearObject.FindChildWithName<UiWillController>("Will Recovery");

            _tabs.Clear();
            CreateTab("Armour", _armourUpgradeController);
            CreateTab("Accessories", _accessoryController);
            CreateTab("Weapons", _weaponUpgradeController);
            if(!isCombat) CreateTab("Crafting", _craftingController);
            CreateTab("Consumables", _consumableController);
            CreateTab("Journals", _journalController);
            CreateTab("Will Recovery", _willController);

            for (int i = 0; i < _tabs.Count; ++i)
            {
                int prevIndex = i - 1;
                Tab prevTab = prevIndex == -1 ? null : _tabs[prevIndex];

                int nextIndex = i + 1;
                Tab nextTab = nextIndex == _tabs.Count ? null : _tabs[nextIndex];

                _tabs[i].SetNeighbors(prevTab, nextTab);
            }
        }

        public static void FlashCloseButton()
        {
            _closeButton.Flash();
        }

        public static void Close()
        {
            FlashCloseButton();
            _currentTab.Deselect();
            _currentMenuController.Hide();
            InputHandler.UnregisterInputListener(_instance);
            MenuStateMachine.ReturnToDefault();
            _open = false;
            CombatManager.Unpause();
            DOTween.defaultTimeScaleIndependent = false;
        }

        public override void Enter()
        {
            base.Enter();
            InputHandler.RegisterInputListener(this);
            DOTween.defaultTimeScaleIndependent = true;
            CombatManager.Pause();
        }

        private static void OpenInventoryMenu(UiInventoryMenuController menu)
        {
            if (!_open)
            {
                _currentMenuController = menu;
                MenuStateMachine.ShowMenu("Inventories", _currentMenuController.Show);
                _open = true;
            }
            else
            {
                _currentMenuController.Hide();
                _currentMenuController = menu;
                _currentMenuController.Show();
            }
        }

        private static void SelectTab(int tabNumber)
        {
            _tabs[tabNumber].Select();
        }

        public static void ShowArmourMenu() => SelectTab(0);

        public static void ShowAccessoryMenu() => SelectTab(1);

        public static void ShowWeaponMenu() => SelectTab(2);

        public static void ShowCraftingMenu() => SelectTab(3);

        public static void ShowConsumableMenu() => SelectTab(4);

        public static void ShowJournalMenu() => SelectTab(5);
    }
}