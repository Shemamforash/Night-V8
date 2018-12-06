using System.Collections.Generic;
using System.Xml;
using DG.Tweening;
using Facilitating.Persistence;
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
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Facilitating.UIControllers
{
    public class UiGearMenuController : Menu, IInputListener
    {
        private UiAccessoryController _accessoryController;
        private UiArmourUpgradeController _armourUpgradeController;
        private UiWeaponUpgradeController _weaponUpgradeController;
        private UICraftingController _craftingController;
        private UiConsumableController _consumableController;
        private UiJournalController _journalController;
        private UiInventoryMenuController _currentMenuController;
        private UiWillController _willController;
        private static UiGearMenuController _instance;
        private CloseButtonController _closeButton;
        private TabController _leftTab, _rightTab;

        private bool _open;
        private readonly List<InventoryTab> _tabs = new List<InventoryTab>();
        private GameObject _tabParent;
        private AudioPoolController _audioPool;

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (isHeld) return;
            switch (axis)
            {
                case InputAxis.SwitchTab:
                    if (direction < 0) InventoryTab.CurrentTab().SelectPreviousTab();
                    else InventoryTab.CurrentTab().SelectNextTab();
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
            InventoryTab tab = _tabParent.FindChildWithName(tabName).FindChildWithName<InventoryTab>("Image");
            tab.SetMenu(menu);
            if (!menu.Unlocked())
            {
                tab.transform.parent.gameObject.SetActive(false);
                return;
            }
            _tabs.Add(tab);
        }

        public static void Save(XmlNode root)
        {
            root = root.CreateChild("Inventories");
            if (_instance == null) return;
            _instance._accessoryController.Save(root);
            _instance._armourUpgradeController.Save(root);
            _instance._weaponUpgradeController.Save(root);
            _instance._craftingController.Save(root);
            _instance._consumableController.Save(root);
            _instance._journalController.Save(root);
        }

        public static void Load(XmlNode root)
        {
            root = root.SelectSingleNode("Inventories");
            _instance._accessoryController.Load(root);
            _instance._armourUpgradeController.Load(root);
            _instance._weaponUpgradeController.Load(root);
            _instance._craftingController.Load(root);
            _instance._consumableController.Load(root);
            _instance._journalController.Load(root);
        }

        public void OnDestroy()
        {
            _instance = null;
        }

        public override void Awake()
        {
            base.Awake();
            bool isCombat = SceneManager.GetActiveScene().name == "Combat";

            _instance = this;
            _audioPool = GetComponent<AudioPoolController>();
            _audioPool.SetMixerGroup("Modified", 0);
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
            if (!isCombat) CreateTab("Crafting", _craftingController);
            else _tabParent.FindChildWithName("Crafting").SetActive(false);
            CreateTab("Consumables", _consumableController);
            CreateTab("Journals", _journalController);
            CreateTab("Will Recovery", _willController);

            for (int i = 0; i < _tabs.Count; ++i)
            {
                int prevIndex = i - 1;
                InventoryTab prevTab = prevIndex == -1 ? null : _tabs[prevIndex];

                int nextIndex = i + 1;
                InventoryTab nextTab = nextIndex == _tabs.Count ? null : _tabs[nextIndex];

                _tabs[i].SetNeighbors(prevTab, nextTab);
            }
        }

        public static void SetCloseButtonAction(UnityAction a)
        {
            _instance._closeButton.SetOnClick(a);
        }

        public static void FlashCloseButton()
        {
            _instance._closeButton.Flash();
        }

        public static void Close()
        {
            if (TutorialManager.IsTutorialVisible()) return;
            FlashCloseButton();
            InventoryTab.ClearActiveTab();
            _instance._currentMenuController.Hide();
            InputHandler.UnregisterInputListener(_instance);
            MenuStateMachine.ReturnToDefault();
            _instance._open = false;
            CombatManager.Resume();
            DOTween.defaultTimeScaleIndependent = false;
        }

        public override void Enter()
        {
            base.Enter();
            InputHandler.RegisterInputListener(this);
            DOTween.defaultTimeScaleIndependent = true;
            CombatManager.Pause();
        }

        public static void OpenInventoryMenu(UiInventoryMenuController menu)
        {
            if (!_instance._open)
            {
                _instance._currentMenuController = menu;
                MenuStateMachine.ShowMenu("Inventories", _instance._currentMenuController.Show);
                _instance._open = true;
            }
            else
            {
                _instance._currentMenuController.Hide();
                _instance._currentMenuController = menu;
                _instance._currentMenuController.Show();
            }
        }

        private static void SelectTab(int tabNumber)
        {
            _instance._tabs[tabNumber].Select();
        }

        public static void ShowArmourMenu() => SelectTab(0);

        public static void ShowAccessoryMenu() => SelectTab(1);

        public static void ShowWeaponMenu() => SelectTab(2);

        public static void ShowCraftingMenu() => SelectTab(3);

        public static void ShowConsumableMenu() => SelectTab(4);

        public static void ShowJournalMenu() => SelectTab(5);

        public static TabController RightTab()
        {
            return _instance._rightTab;
        }

        public static TabController LeftTab()
        {
            return _instance._leftTab;
        }

        public static void PlayAudio(AudioClip clip)
        {
            _instance._audioPool.Create().Play(clip, Random.Range(0.9f, 1f), Random.Range(0.8f, 1f));
        }

        public static void PlayTabAudio()
        {
            _instance._audioPool.Create().Play(AudioClips.TabChange, Random.Range(0.9f, 1f), Random.Range(0.9f, 1f));
        }

        public static void ShowMeditateMenu()
        {
            SelectTab(6);
        }
    }
}