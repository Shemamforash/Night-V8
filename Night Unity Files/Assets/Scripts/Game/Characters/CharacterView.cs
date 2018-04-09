using System.Collections.Generic;
using System.Linq;
using Facilitating.UIControllers;
using Game.Characters.CharacterActions;
using Game.Global;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Characters
{
    public class CharacterView : MonoBehaviour
    {
        private Player _player;
        private MenuList _actionMenuList;
        private TextMeshProUGUI _currentActionText;
        private TextMeshProUGUI _detailedCurrentActionText;
        private GameObject _detailedView;
        private GameObject SimpleView;
        public UIPlayerAccessoryController AccessoryController;
        public UIPlayerArmourController ArmourController;
        public UIPlayerWeaponController WeaponController;

        public void SetPlayer(Player player)
        {
            _player = player;
            _player.CharacterView = this;
            CacheSimpleViewElements();
            CacheDetailedViewElements();
            SwitchToSimpleView();
            BindUi();
            FillActionList();
        }

        private void BindUi()
        {
            FindInSimpleView<UIAttributeController>("Attributes").HookValues(_player.Attributes);
            FindInDetailedView<UIAttributeController>("Attributes").HookValues(_player.Attributes);
            FindInDetailedView<UIConditionController>("Thirst").HookDehydration(_player.Attributes);
            FindInDetailedView<UIConditionController>("Hunger").HookStarvation(_player.Attributes);
            FindInSimpleView<UIConditionController>("Thirst").HookDehydration(_player.Attributes);
            FindInSimpleView<UIConditionController>("Hunger").HookStarvation(_player.Attributes);
        }

        private void CacheSimpleViewElements()
        {
            SimpleView = transform.Find("Simple").gameObject;
            SimpleView.SetActive(true);
            FindInSimpleView<TextMeshProUGUI>("Name").text = _player.Name;
            _currentActionText = FindInSimpleView<TextMeshProUGUI>("Current Action");
        }

        private void CacheDetailedViewElements()
        {
            _detailedView = transform.Find("Detailed").gameObject;
            _detailedView.SetActive(true);
            _detailedView.SetActive(false);

            _actionMenuList = FindInDetailedView<MenuList>("Action List");

            _detailedCurrentActionText = FindInDetailedView<TextMeshProUGUI>("CurrentAction");

            FindInDetailedView<TextMeshProUGUI>("Detailed Name").text = _player.Name;

            WeaponController = FindInDetailedView<UIPlayerWeaponController>("Weapon");
            WeaponController.EnhancedButton.AddOnClick(() => UiGearMenuController.Instance().ShowWeaponMenu(_player));
            ArmourController = FindInDetailedView<UIPlayerArmourController>("Armour");
            ArmourController.EnhancedButton.AddOnClick(() => UiGearMenuController.Instance().ShowArmourMenu(_player));
            _player.ArmourController.AddOnArmourChange(() => ArmourController.SetArmour(_player.ArmourController));
            AccessoryController = FindInDetailedView<UIPlayerAccessoryController>("Accessory");
            AccessoryController.EnhancedButton.AddOnClick(() => UiGearMenuController.Instance().ShowAccessoryMenu(_player));
        }

        private void FillActionList()
        {
            List<BaseCharacterAction> availableActions = _player.StatesAsList(false).ToList();
            _actionMenuList.SetItems(new List<MyGameObject>(availableActions), false);
            List<ViewParent> actionUiList = _actionMenuList.Items;

            for (int i = 0; i < actionUiList.Count; ++i)
            {
                ViewParent actionUi = actionUiList[i];

                Helper.SetNavigation(actionUi.PrimaryButton, WeaponController.EnhancedButton, Direction.Left);
                if (i != 0) continue;
                Helper.SetNavigation(WeaponController.EnhancedButton, actionUi.PrimaryButton, Direction.Right);
                Helper.SetNavigation(ArmourController.EnhancedButton.Button(), actionUi.PrimaryButton, Direction.Right);
                Helper.SetNavigation(AccessoryController.EnhancedButton.Button(), actionUi.PrimaryButton, Direction.Right);
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
                    if(_actionListActive) GetFirstActionButton().Button().Select();
                    else WeaponController.EnhancedButton.Button().Select();
                });
                GetFirstActionButton().SetOnUpAction(() =>
                {
                    CharacterManager.ExitCharacter(_player);
                    WorldView.GetInventoryButton().Button().Select();
                });
                WeaponController.EnhancedButton.SetOnUpAction(() =>
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
                    if(_actionListActive) GetFirstActionButton().Button().Select();
                    else WeaponController.EnhancedButton.Button().Select();
                });
                GetFirstActionButton().SetOnUpAction(() =>
                {
                    CharacterManager.ExitCharacter(_player);
                    CharacterManager.SelectCharacter(_previousCharacterView._player);
                    if(_previousCharacterView._actionListActive) _previousCharacterView.GetLastActionButton().Button().Select();
                    else _previousCharacterView.ArmourController.EnhancedButton.Button().Select();
                });
                WeaponController.EnhancedButton.SetOnUpAction(() =>
                {
                    CharacterManager.ExitCharacter(_player);
                    CharacterManager.SelectCharacter(_previousCharacterView._player);
                    _previousCharacterView.ArmourController.EnhancedButton.Button().Select();
                });
                _previousCharacterView.ArmourController.EnhancedButton.SetOnDownAction(() =>
                {
                    CharacterManager.ExitCharacter(_previousCharacterView._player);
                    CharacterManager.SelectCharacter(_player);
                    WeaponController.EnhancedButton.Button().Select();
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

        public void UpdateCurrentActionText(string currentActionString)
        {
            BaseCharacterAction currentState = (BaseCharacterAction) _player.States.GetCurrentState();
            if (currentState == null) return;
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

        private bool _actionListActive = true;
        
        private void SetActionListActive(bool active)
        {
            RefreshNavigation();
            _actionListActive = active;
            _actionMenuList.InventoryContent.gameObject.SetActive(active);
            _detailedCurrentActionText.gameObject.SetActive(!active);
        }

        private T FindInSimpleView<T>(string elementName) where T : class
        {
            return Helper.FindChildWithName<T>(SimpleView, elementName);
        }

        private T FindInDetailedView<T>(string elementName) where T : class
        {
            return Helper.FindChildWithName<T>(_detailedView, elementName);
        }

        public void SwitchToDetailedView()
        {
            GetComponent<LayoutElement>().preferredHeight = 200;
            _detailedView.SetActive(true);
            SimpleView.SetActive(false);
            _actionMenuList.Items[0].Select();
        }

        public void SwitchToSimpleView()
        {
            GetComponent<LayoutElement>().preferredHeight = 60;
            _detailedView.SetActive(false);
            SimpleView.SetActive(true);
//            SimpleView.GetComponent<Button>().Select();
        }
    }
}