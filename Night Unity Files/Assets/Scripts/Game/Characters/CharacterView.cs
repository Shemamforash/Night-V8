using System.Collections.Generic;
using System.Linq;
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
        private readonly Player.Player _character;
        public readonly GameObject GameObject;
        private GameObject SimpleView;
        private GameObject _detailedView;

        private TextMeshProUGUI _currentActionText;
        private TextMeshProUGUI _detailedCurrentActionText;

        public UIGearController WeaponGearUi, ArmourGearUi, AccessoryGearUi;

        private MenuList _actionMenuList;

        private void BindUi()
        {
            FindInSimpleView<UIAttributeController>("Attributes").HookValues(_character.Attributes);
            FindInDetailedView<UIAttributeController>("Attributes").HookValues(_character.Attributes);
            UIEnergyController energyController = FindInDetailedView<UIEnergyController>("Energy");
            _character.Energy.AddOnValueChange(a => { energyController.SetValue((int) a.CurrentValue(), (int) a.Max); });

            FindInDetailedView<UIConditionController>("Thirst").HookDehydration(_character.Attributes);
            FindInDetailedView<UIConditionController>("Hunger").HookStarvation(_character.Attributes);
            FindInSimpleView<UIConditionController>("Thirst").HookDehydration(_character.Attributes);
            FindInSimpleView<UIConditionController>("Hunger").HookStarvation(_character.Attributes);
        }

        private void CacheSimpleViewElements()
        {
            SimpleView = GameObject.transform.Find("Simple").gameObject;
            SimpleView.SetActive(true);

            FindInSimpleView<TextMeshProUGUI>("Simple Name").text = _character.Name;
            FindInSimpleView<TextMeshProUGUI>("ClassTrait").text = "fill me in";

            _currentActionText = FindInSimpleView<TextMeshProUGUI>("Current Action");
        }

        private void CacheDetailedViewElements()
        {
            _detailedView = GameObject.transform.Find("Detailed").gameObject;
            _detailedView.SetActive(true);
            _detailedView.SetActive(false);

            _actionMenuList = FindInDetailedView<MenuList>("Action List");

            _detailedCurrentActionText = FindInDetailedView<TextMeshProUGUI>("CurrentAction");

            FindInDetailedView<TextMeshProUGUI>("Detailed Name").text = _character.Name;
            FindInDetailedView<TextMeshProUGUI>("Class").text = "fill me in";
            FindInDetailedView<TextMeshProUGUI>("Trait").text = "fill me in";
            FindInDetailedView<TextMeshProUGUI>("Weight").text = "fill me in";

            WeaponGearUi = FindInDetailedView<UIGearController>("Weapon");
            ArmourGearUi = FindInDetailedView<UIGearController>("Armour");
            AccessoryGearUi = FindInDetailedView<UIGearController>("Accessory");
        }

        public void FillActionList()
        {
            List<BaseCharacterAction> availableActions = _character.StatesAsList(false).ToList();
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
            EnhancedButton weaponGearUiButton = WeaponGearUi.GetComponent<EnhancedButton>();
            EnhancedButton armourGearUiButton = ArmourGearUi.GetComponent<EnhancedButton>();
            EnhancedButton accessoryGearuiButton = AccessoryGearUi.GetComponent<EnhancedButton>();

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
            CharacterView _previousCharacterView = CharacterManager.PreviousCharacter(_character)?.CharacterView;

            if (_previousCharacterView == null)
            {
                WorldView.GetInventoryButton().SetOnDownAction(() =>
                {
                    CharacterManager.SelectCharacter(_character);
                    _actionMenuList.Items[0].PrimaryButton.Button().Select();
                });
                _actionMenuList.Items[0].PrimaryButton.SetOnUpAction(() =>
                {
                    CharacterManager.ExitCharacter(_character);
                    WorldView.GetInventoryButton().Button().Select();
                });
                WeaponGearUi.GetComponent<EnhancedButton>().SetOnUpAction(() =>
                {
                    CharacterManager.ExitCharacter(_character);
                    WorldView.GetInventoryButton().Button().Select();
                });
            }
            else
            {
                _previousCharacterView.GetLastActionButton().SetOnDownAction(() =>
                {
                    CharacterManager.ExitCharacter(_previousCharacterView._character);
                    CharacterManager.SelectCharacter(_character);
                    _actionMenuList.Items[0].PrimaryButton.Button().Select();
                });
                GetFirstActionButton().SetOnUpAction(() =>
                {
                    CharacterManager.ExitCharacter(_character);
                    CharacterManager.SelectCharacter(_previousCharacterView._character);
                    _previousCharacterView.GetLastActionButton().Button().Select();
                });
                WeaponGearUi.GetComponent<EnhancedButton>().SetOnUpAction(() =>
                {
                    CharacterManager.ExitCharacter(_character);
                    CharacterManager.SelectCharacter(_previousCharacterView._character);
                    _previousCharacterView.AccessoryGearUi.GetComponent<EnhancedButton>().Button().Select();
                });
                _previousCharacterView.AccessoryGearUi.GetComponent<EnhancedButton>().SetOnDownAction(() =>
                {
                    CharacterManager.ExitCharacter(_previousCharacterView._character);
                    CharacterManager.SelectCharacter(_character);
                    WeaponGearUi.GetComponent<EnhancedButton>().Button().Select();
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
            return CharacterManager.PreviousCharacter(_character)?.CharacterView.SimpleView.GetComponent<EnhancedButton>();
        }

        private EnhancedButton GetCharacterButtonBelow()
        {
            return CharacterManager.NextCharacter(_character)?.CharacterView.SimpleView.GetComponent<EnhancedButton>();
        }

        public CharacterView(Player.Player character)
        {
            _character = character;
            GameObject = _character.GetGameObject();
            GameObject.SetActive(true);
            CacheSimpleViewElements();
            CacheDetailedViewElements();
            BindUi();
            WorldState.RegisterMinuteEvent(UpdateCurrentActionText);
            SwitchToSimpleView();
        }

        private void UpdateCurrentActionText()
        {
            BaseCharacterAction currentState = (BaseCharacterAction) _character.States.GetCurrentState();
            if (currentState == null) return;
            string currentActionString = currentState.GetActionText();
            _currentActionText.text = currentActionString;
            if (currentState.Name == "Idle")
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