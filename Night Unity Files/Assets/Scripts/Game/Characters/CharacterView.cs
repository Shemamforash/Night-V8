using System.Collections.Generic;
using System.Linq;
using Game.Characters.CharacterActions;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Characters
{
    public class CharacterView
    {
        private readonly Player _character;
        public readonly GameObject GameObject;
        public GameObject SimpleView;
        private GameObject _detailedView;

        private TextMeshProUGUI _currentActionText;
        private TextMeshProUGUI _detailedCurrentActionText;

        public UIGearController WeaponGearUi, ArmourGearUi, AccessoryGearUi;

        private MenuList _actionMenuList;

        private void BindUi()
        {
            FindInSimpleView<UIAttributeController>("Attributes").HookValues(_character.BaseAttributes);
            FindInDetailedView<UIAttributeController>("Attributes").HookValues(_character.BaseAttributes);
            _character.Energy.AddOnValueChange(a => FindInDetailedView<UIEnergyController>("Energy").SetValue((int) a.CurrentValue()));

            FindInDetailedView<UIConditionController>("Thirst").HookDehydration(_character.SurvivalAttributes);
            FindInDetailedView<UIConditionController>("Hunger").HookStarvation(_character.SurvivalAttributes);
            FindInSimpleView<UIConditionController>("Thirst").HookDehydration(_character.SurvivalAttributes);
            FindInSimpleView<UIConditionController>("Hunger").HookStarvation(_character.SurvivalAttributes);
        }

        private void CacheSimpleViewElements()
        {
            SimpleView = GameObject.transform.Find("Simple").gameObject;
            SimpleView.SetActive(true);

            FindInSimpleView<TextMeshProUGUI>("Simple Name").text = _character.Name;
            FindInSimpleView<TextMeshProUGUI>("ClassTrait").text = _character.CharacterTrait.Name + " " + _character.CharacterClass.Name;

            _currentActionText = FindInSimpleView<TextMeshProUGUI>("Current Action");
        }

        private void CacheDetailedViewElements()
        {
            _detailedView = GameObject.transform.Find("Detailed").gameObject;
            _detailedView.SetActive(false);

            _actionMenuList = FindInDetailedView<MenuList>("Action List");

            _detailedCurrentActionText = FindInDetailedView<TextMeshProUGUI>("CurrentAction");

            FindInDetailedView<TextMeshProUGUI>("Detailed Name").text = _character.Name;
            FindInDetailedView<TextMeshProUGUI>("Class").text = _character.CharacterClass.GetTraitDetails();
            FindInDetailedView<TextMeshProUGUI>("Trait").text = _character.CharacterTrait.GetTraitDetails();
            FindInDetailedView<TextMeshProUGUI>("Weight").text = "Weight: " + _character.SurvivalAttributes.Weight + " (requires " + ((int) _character.SurvivalAttributes.Weight + 5) + "fuel)";

            WeaponGearUi = FindInDetailedView<UIGearController>("Weapon");
            ArmourGearUi = FindInDetailedView<UIGearController>("Armour");
            AccessoryGearUi = FindInDetailedView<UIGearController>("Accessory");
        }

        public void ResetGearToActionNavigation()
        {
            List<BaseCharacterAction> availableActions = _character.StatesAsList(false).ToList();
            _actionMenuList.SetItems(new List<MyGameObject>(availableActions));

            List<ViewParent> actionUiList = _actionMenuList.Items;
            for (int i = 0; i < actionUiList.Count; ++i)
            {
                ViewParent actionUi = actionUiList[i];

                Button weaponGearUiButton = WeaponGearUi.GetComponent<Button>();
                Helper.SetNavigation(actionUi.GetNavigationButton(), weaponGearUiButton, Direction.Left);
                if (i != 0) continue;
                Helper.SetNavigation(weaponGearUiButton, actionUi.GetNavigationButton(), Direction.Right);
                Helper.SetNavigation(ArmourGearUi.GetComponent<Button>(), actionUi.GetNavigationButton(), Direction.Right);
                Helper.SetNavigation(AccessoryGearUi.GetComponent<Button>(), actionUi.GetNavigationButton(), Direction.Right);
            }
        }

        public CharacterView(Player character)
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
            BaseCharacterAction currentState = _character.States.GetCurrentState();
            if (currentState == null) return;
            string currentActionString = currentState.Name + " " + currentState.GetCostAsString();
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
            _actionMenuList.Items[0].GetNavigationButton().Select();
        }

        public void SwitchToSimpleView()
        {
            GameObject.GetComponent<LayoutElement>().preferredHeight = 60;
            _detailedView.SetActive(false);
            SimpleView.SetActive(true);
            SimpleView.GetComponent<Button>().Select();
        }
    }
}