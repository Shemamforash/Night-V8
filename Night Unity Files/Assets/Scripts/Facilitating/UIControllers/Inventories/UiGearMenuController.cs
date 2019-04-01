using System.Collections.Generic;
using System.Linq;
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
        private static bool _closeAllowed = true, _openAllowed = true;

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
            _tabs.Add(tab);
        }

        private void UpdateTabs()
        {
            _tabs.ForEach(t => { t.UpdateActive(); });
            for (int i = 0; i < _tabs.Count; ++i)
            {
                InventoryTab prevTab = null;
                int prevIndex = i - 1;
                while (prevTab == null && prevIndex >= 0)
                {
                    if (_tabs[prevIndex].Active()) prevTab = _tabs[prevIndex];
                    --prevIndex;
                }


                int nextIndex = i + 1;
                InventoryTab nextTab = null;
                while (nextTab == null && nextIndex < _tabs.Count)
                {
                    if (_tabs[nextIndex].Active()) nextTab = _tabs[nextIndex];
                    ++nextIndex;
                }

                _tabs[i].SetNeighbors(prevTab, nextTab);
            }
        }

        public static void Save(XmlNode root)
        {
            root = root.CreateChild("Inventories");
            UiAccessoryController.Save(root);
            UiArmourUpgradeController.Save(root);
            UICraftingController.Save(root);
            UiConsumableController.Save(root);
            UiJournalController.Save(root);
        }

        public static void Load(XmlNode root)
        {
            root = root.SelectSingleNode("Inventories");
            UiAccessoryController.Load(root);
            UiArmourUpgradeController.Load(root);
            UICraftingController.Load(root);
            UiConsumableController.Load(root);
            UiJournalController.Load(root);
        }

        public void OnDestroy()
        {
            _instance = null;
        }

        protected override void Awake()
        {
            base.Awake();
            bool isCombat = SceneManager.GetActiveScene().name == "Combat";

            _closeAllowed = true;
            _openAllowed = true;
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
            _consumableController = gearObject.FindChildWithName<UiConsumableController>("Inventory");
            _journalController = gearObject.FindChildWithName<UiJournalController>("Journals");
            _willController = gearObject.FindChildWithName<UiWillController>("Will Recovery");

            _tabs.Clear();
            CreateTab("Armour", _armourUpgradeController);
            CreateTab("Accessories", _accessoryController);
            CreateTab("Weapons", _weaponUpgradeController);
            if (!isCombat) CreateTab("Crafting", _craftingController);
            else _tabParent.FindChildWithName("Crafting").SetActive(false);
            CreateTab("Inventory", _consumableController);
            CreateTab("Journals", _journalController);
            CreateTab("Will Recovery", _willController);
            UpdateTabs();
        }

        private void Start()
        {
            ControlTypeChangeListener controlTypeChangeListener = GetComponent<ControlTypeChangeListener>();
            controlTypeChangeListener.SetOnControllerInputChange(UpdateText);
        }

        private void UpdateText()
        {
            string text = InputHandler.GetBindingForKey(InputAxis.SwitchTab);
            string leftText = "J";
            string rightText = "L";
            if (text != "J - L")
            {
                leftText = "<";
                rightText = ">";
            }

            _leftTab.SetText(leftText);
            _rightTab.SetText(rightText);
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
            if (!_closeAllowed) return;
            if (TutorialManager.Instance.IsTutorialVisible()) return;
            FlashCloseButton();
            InventoryTab.ClearActiveTab();
            _instance._currentMenuController.Hide();
            InputHandler.UnregisterInputListener(_instance);
            MenuStateMachine.ReturnToDefault();
            _instance._open = false;
            WorldState.Resume();
            DOTween.defaultTimeScaleIndependent = false;
            ButtonClickListener.SuppressClick();
        }

        public override void Enter()
        {
            base.Enter();
            InputHandler.RegisterInputListener(this);
            DOTween.defaultTimeScaleIndependent = true;
            WorldState.Pause();
            ButtonClickListener.SuppressClick();
            AudioController.FadeInMusicMuffle();
            AudioController.FadeOutCombat();
        }

        public override void Exit()
        {
            AudioController.FadeOutMusicMuffle();
            AudioController.FadeInCombat();
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
            _instance.UpdateTabs();
            _instance._tabs[tabNumber].Select();
        }

        public static void ShowInventories()
        {
            if (!_openAllowed) return;
            for (int i = 0; i < _instance._tabs.Count; i++)
            {
                InventoryTab tab = _instance._tabs[i];
                tab.UpdateActive();
                if (!tab.Active()) continue;
                SelectTab(i);
                break;
            }
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
            _instance._audioPool.Create(false).Play(clip, Random.Range(0.9f, 1f), Random.Range(0.8f, 1f));
        }

        public static void ShowMeditateMenu()
        {
            SelectTab(6);
        }

        public static bool IsOpen() => MenuStateMachine.CurrentMenu() == _instance;

        public static void SetCloseAllowed(bool allowed) => _closeAllowed = allowed;

        public static void SetOpenAllowed(bool allowed) => _openAllowed = allowed;
    }
}