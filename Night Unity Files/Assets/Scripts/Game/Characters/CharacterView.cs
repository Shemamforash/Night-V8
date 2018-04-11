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
        private Transform _actionList;
        private TextMeshProUGUI _currentActionText;
        private TextMeshProUGUI _detailedCurrentActionText;
        private GameObject _detailedView;
        private GameObject SimpleView;
        public UIPlayerAccessoryController AccessoryController;
        public UIPlayerArmourController ArmourController;
        public UIPlayerWeaponController WeaponController;
        private UIAttributeController _attributeControllerSimple;
        private UIAttributeController _attributeControllerDetailed;
        private UIConditionController _thirstControllerSimple, _hungerControllerSimple;
        private UIConditionController _thirstControllerDetailed, _hungerControllerDetailed;

        public void SetPlayer(Player player)
        {
            _player = player;
            _player.CharacterView = this;
            CacheSimpleViewElements();
            CacheDetailedViewElements();
            SwitchToSimpleView();
            BindUi();
            FillActionList();
            ((BaseCharacterAction) _player.States.GetCurrentState()).UpdateActionText();
        }

        private void BindUi()
        {
            _attributeControllerSimple = FindInSimpleView<UIAttributeController>("Attributes");
            _attributeControllerDetailed = FindInDetailedView<UIAttributeController>("Attributes");

            _thirstControllerDetailed = FindInDetailedView<UIConditionController>("Thirst");
            _hungerControllerDetailed = FindInDetailedView<UIConditionController>("Hunger");
            _thirstControllerSimple = FindInSimpleView<UIConditionController>("Thirst");
            _hungerControllerSimple = FindInSimpleView<UIConditionController>("Hunger");
        }

        public void Update()
        {
            _attributeControllerSimple.UpdateAttributes(_player);
            _attributeControllerDetailed.UpdateAttributes(_player);
            _thirstControllerDetailed.UpdateDehydration(_player);
            _hungerControllerDetailed.UpdateHunger(_player);
            _thirstControllerSimple.UpdateDehydration(_player);
            _hungerControllerSimple.UpdateHunger(_player);
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

            _actionList = FindInDetailedView<Transform>("Action List");
            _detailedCurrentActionText = FindInDetailedView<TextMeshProUGUI>("CurrentAction");

            FindInDetailedView<TextMeshProUGUI>("Detailed Name").text = _player.Name;

            WeaponController = FindInDetailedView<UIPlayerWeaponController>("Weapon");
            WeaponController.EnhancedButton.AddOnClick(() => UiGearMenuController.Instance().ShowWeaponMenu(_player));
            WeaponController.SetWeapon(_player.Weapon);
            
            ArmourController = FindInDetailedView<UIPlayerArmourController>("Armour");
            ArmourController.EnhancedButton.AddOnClick(() => UiGearMenuController.Instance().ShowArmourMenu(_player));
            ArmourController.SetArmour(_player.ArmourController);
            _player.ArmourController.AddOnArmourChange(() => ArmourController.SetArmour(_player.ArmourController));

            AccessoryController = FindInDetailedView<UIPlayerAccessoryController>("Accessory");
            AccessoryController.SetAccessory(_player.Accessory);
            AccessoryController.EnhancedButton.AddOnClick(() => UiGearMenuController.Instance().ShowAccessoryMenu(_player));
        }

        private void FillActionList()
        {
            List<BaseCharacterAction> availableActions = _player.StatesAsList(false).ToList();
            Helper.FindAllChildren(_actionList).ForEach(Destroy);
            List<ViewParent> actionUiList = new List<ViewParent>();
            availableActions.ForEach(a =>
            {
                ViewParent actionUi = a.CreateUi(_actionList);
                actionUiList.Add(actionUi);
            });

            for (int i = 0; i < actionUiList.Count; ++i)
            {
                ViewParent actionUi = actionUiList[i];

                Helper.SetNavigation(actionUi.PrimaryButton, WeaponController.EnhancedButton, Direction.Left);
                if (i > 0)
                {
                    Helper.SetNavigation(actionUi.PrimaryButton, actionUiList[i - 1].PrimaryButton, Direction.Up);
                    Helper.SetNavigation(actionUiList[i - 1].PrimaryButton, actionUi.PrimaryButton, Direction.Down);
                }
                else
                {
                    Helper.SetNavigation(WeaponController.EnhancedButton, actionUi.PrimaryButton, Direction.Right);
                    Helper.SetNavigation(ArmourController.EnhancedButton.Button(), actionUi.PrimaryButton, Direction.Right);
                    Helper.SetNavigation(AccessoryController.EnhancedButton.Button(), actionUi.PrimaryButton, Direction.Right);
                }
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
                    if (_actionListActive) GetFirstActionButton().Button().Select();
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
                    if (_actionListActive) GetFirstActionButton().Button().Select();
                    else WeaponController.EnhancedButton.Button().Select();
                });
                GetFirstActionButton().SetOnUpAction(() =>
                {
                    CharacterManager.ExitCharacter(_player);
                    CharacterManager.SelectCharacter(_previousCharacterView._player);
                    if (_previousCharacterView._actionListActive) _previousCharacterView.GetLastActionButton().Button().Select();
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
            return _actionList.GetChild(_actionList.childCount - 1).GetComponent<EnhancedButton>();
        }

        private EnhancedButton GetFirstActionButton()
        {
            return _actionList.GetChild(0).GetComponent<EnhancedButton>();
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
            _actionList.gameObject.SetActive(active);
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
            _actionList.GetChild(0).GetComponent<EnhancedButton>().Button().Select();
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