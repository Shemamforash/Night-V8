using System.Collections.Generic;
using System.Linq;
using Facilitating.UIControllers;
using Game.Characters.CharacterActions;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Characters
{
    public class CharacterView
    {
        private readonly Player.Player _player;
        public readonly GameObject GameObject;
        private GameObject SimpleView;
        private GameObject _detailedView;

        private TextMeshProUGUI _currentActionText;
        private TextMeshProUGUI _detailedCurrentActionText;
        public UIPlayerAccessoryController AccessoryController;
        public UIPlayerWeaponController WeaponController;
        public UIPlayerArmourController ArmourController;

        private MenuList _actionMenuList;

        private void BindUi()
        {
            FindInSimpleView<UIAttributeController>("Attributes").HookValues(_player.Attributes);
            FindInDetailedView<UIAttributeController>("Attributes").HookValues(_player.Attributes);
            UIEnergyController energyController = FindInDetailedView<UIEnergyController>("Energy");
            _player.Energy.AddOnValueChange(a => { energyController.SetValue((int) a.CurrentValue(), (int) a.Max); });

            FindInDetailedView<UIConditionController>("Thirst").HookDehydration(_player.Attributes);
            FindInDetailedView<UIConditionController>("Hunger").HookStarvation(_player.Attributes);
            FindInSimpleView<UIConditionController>("Thirst").HookDehydration(_player.Attributes);
            FindInSimpleView<UIConditionController>("Hunger").HookStarvation(_player.Attributes);
        }

        private void CacheSimpleViewElements()
        {
            SimpleView = GameObject.transform.Find("Simple").gameObject;
            SimpleView.SetActive(true);

            FindInSimpleView<TextMeshProUGUI>("Name").text = _player.Name;

            _currentActionText = FindInSimpleView<TextMeshProUGUI>("Current Action");
        }

        private void CacheDetailedViewElements()
        {
            _detailedView = GameObject.transform.Find("Detailed").gameObject;
            _detailedView.SetActive(true);
            _detailedView.SetActive(false);

            _actionMenuList = FindInDetailedView<MenuList>("Action List");

            _detailedCurrentActionText = FindInDetailedView<TextMeshProUGUI>("CurrentAction");

            FindInDetailedView<TextMeshProUGUI>("Detailed Name").text = _player.Name;

            WeaponController = FindInDetailedView<UIPlayerWeaponController>("Weapon");
            WeaponController.GetComponent<EnhancedButton>().AddOnClick(() => UiWeaponUpgradeController.Show(_player));
            ArmourController = FindInDetailedView<UIPlayerArmourController>("Armour");
            _player.ArmourController.AddOnArmourChange(a => ArmourController.SetArmour(_player.ArmourController));
            AccessoryController = FindInDetailedView<UIPlayerAccessoryController>("Accessory");
        }

        public void FillActionList()
        {
            List<BaseCharacterAction> availableActions = _player.StatesAsList(false).ToList();
            _actionMenuList.SetItems(new List<MyGameObject>(availableActions), false);
//            foreach (ViewParent action in _actionMenuList.Items)
//            {
//                action.PrimaryButton.AddOnSelectEvent(() =>
//                {
//                    CharacterManager.SelectCharacter(_character);
//                });
//                action.PrimaryButton.AddOnDeselectEvent(() => CharacterManager.ExitCharacter(_character));
//            }

            List<ViewParent> actionUiList = _actionMenuList.Items;
            EnhancedButton weaponGearUiButton = WeaponController.GetComponent<EnhancedButton>();
            EnhancedButton armourGearUiButton = ArmourController.GetComponent<EnhancedButton>();
            EnhancedButton accessoryGearuiButton = AccessoryController.GetComponent<EnhancedButton>();

            for (int i = 0; i < actionUiList.Count; ++i)
            {
                ViewParent actionUi = actionUiList[i];

                Helper.SetNavigation(actionUi.PrimaryButton, weaponGearUiButton, Direction.Left);
                if (i != 0) continue;
                Helper.SetNavigation(weaponGearUiButton, actionUi.PrimaryButton, Direction.Right);
                Helper.SetNavigation(armourGearUiButton.GetComponent<Button>(), actionUi.PrimaryButton, Direction.Right);
                Helper.SetNavigation(accessoryGearuiButton.GetComponent<Button>(), actionUi.PrimaryButton, Direction.Right);
            }
        }

        //actions[0], weaponui, & consumption toggle navigate to actions[last], accessoryui, & consumption buttons respectively
        //inverse is true to navigate to character below
        //if no character above, all navigate to inventory button
        //if no character below do nothing
        public void RefreshNavigation()
        {
            CharacterView _previousCharacterView = CharacterManager.PreviousCharacter(_player)?.CharacterView;

            if (_previousCharacterView == null)
            {
                WorldView.GetInventoryButton().SetOnDownAction(() =>
                {
                    CharacterManager.SelectCharacter(_player);
                    _actionMenuList.Items[0].PrimaryButton.Button().Select();
                });
                _actionMenuList.Items[0].PrimaryButton.SetOnUpAction(() =>
                {
                    CharacterManager.ExitCharacter(_player);
                    WorldView.GetInventoryButton().Button().Select();
                });
                WeaponController.GetComponent<EnhancedButton>().SetOnUpAction(() =>
                {
                    CharacterManager.ExitCharacter(_player);
                    WorldView.GetInventoryButton().Button().Select();
                });
            }
            else
            {
                _previousCharacterView.GetLastActionButton().SetOnDownAction(() =>
                {
                    CharacterManager.ExitCharacter(_previousCharacterView._player);
                    CharacterManager.SelectCharacter(_player);
                    _actionMenuList.Items[0].PrimaryButton.Button().Select();
                });
                GetFirstActionButton().SetOnUpAction(() =>
                {
                    CharacterManager.ExitCharacter(_player);
                    CharacterManager.SelectCharacter(_previousCharacterView._player);
                    _previousCharacterView.GetLastActionButton().Button().Select();
                });
                WeaponController.GetComponent<EnhancedButton>().SetOnUpAction(() =>
                {
                    CharacterManager.ExitCharacter(_player);
                    CharacterManager.SelectCharacter(_previousCharacterView._player);
                    _previousCharacterView.ArmourController.GetComponent<EnhancedButton>().Button().Select();
                });
                _previousCharacterView.ArmourController.GetComponent<EnhancedButton>().SetOnDownAction(() =>
                {
                    CharacterManager.ExitCharacter(_previousCharacterView._player);
                    CharacterManager.SelectCharacter(_player);
                    ArmourController.GetComponent<EnhancedButton>().Button().Select();
                });
            }
        }

        private EnhancedButton GetLastActionButton()
        {
            return _actionMenuList.Items[_actionMenuList.Items.Count - 1].PrimaryButton;
        }

        private EnhancedButton GetFirstActionButton()
        {
            return _actionMenuList.Items[0].PrimaryButton;
        }

        private void NavigateToButtonInOtherCharacterView(CharacterView other, EnhancedButton b)
        {
            SwitchToSimpleView();
            other.SwitchToDetailedView();
            b.Button().Select();
        }

        private EnhancedButton GetCharacterButtonAbove()
        {
            return CharacterManager.PreviousCharacter(_player)?.CharacterView.SimpleView.GetComponent<EnhancedButton>();
        }

        private EnhancedButton GetCharacterButtonBelow()
        {
            return CharacterManager.NextCharacter(_player)?.CharacterView.SimpleView.GetComponent<EnhancedButton>();
        }

        public CharacterView(Player.Player player)
        {
            _player = player;
            GameObject = _player.GetGameObject();
            GameObject.SetActive(true);
            CacheSimpleViewElements();
            CacheDetailedViewElements();
            BindUi();
            WorldState.RegisterMinuteEvent(UpdateCurrentActionText);
            SwitchToSimpleView();
        }

        private void UpdateCurrentActionText()
        {
            BaseCharacterAction currentState = (BaseCharacterAction) _player.States.GetCurrentState();
            if (currentState == null) return;
            string currentActionString = currentState.GetActionText();
            _currentActionText.text = currentActionString;
            if (_player.States.IsDefaultState(currentState))
            {
                SetActionListActive(true);
            }
            else
            {
                SetActionListActive(false);
                _detailedCurrentActionText.text = currentActionString;
            }
        }

        private void SetActionListActive(bool active)
        {
            _actionMenuList.InventoryContent.gameObject.SetActive(active);
            _detailedCurrentActionText.gameObject.SetActive(!active);
        }

        private T FindInSimpleView<T>(string name) where T : class
        {
            return Helper.FindChildWithName<T>(SimpleView, name);
        }

        private T FindInDetailedView<T>(string name) where T : class
        {
            return Helper.FindChildWithName<T>(_detailedView, name);
        }

        public void SwitchToDetailedView()
        {
            GameObject.GetComponent<LayoutElement>().preferredHeight = 200;
            _detailedView.SetActive(true);
            SimpleView.SetActive(false);
            _actionMenuList.Items[0].Select();
        }

        public void SwitchToSimpleView()
        {
            GameObject.GetComponent<LayoutElement>().preferredHeight = 60;
            _detailedView.SetActive(false);
            SimpleView.SetActive(true);
//            SimpleView.GetComponent<Button>().Select();
        }
    }
}