using System;
using System.Collections.Generic;
using System.Linq;
using Facilitating.MenuNavigation;
using Game.Characters.Attributes;
using Game.Characters.CharacterActions;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
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
        private readonly Player _character;
        public readonly GameObject GameObject;
        public GameObject SimpleView;
        private GameObject _detailedView;
        private GameObject _gearContainer;

        private readonly ValueTextLink<string> _currentActionText = new ValueTextLink<string>();
        private readonly ValueTextLink<string> _detailedCurrentActionText = new ValueTextLink<string>();

        public UIGearController WeaponGearUi, ArmourGearUi, AccessoryGearUi;

        private MenuList _actionMenuList;

        public void SetActionListActive(bool active)
        {
            _actionMenuList.gameObject.SetActive(active);
            _detailedCurrentActionText.SetEnabled(!active);
        }
        
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
        
        private void SetSimpleView()
        {
            SimpleView = GameObject.transform.Find("Simple").gameObject;
            SimpleView.SetActive(true);

            FindInSimpleView<TextMeshProUGUI>("Simple Name").text = _character.Name;
            FindInSimpleView<TextMeshProUGUI>("ClassTrait").text = _character.CharacterTrait.Name + " " + _character.CharacterClass.Name;
            
            _currentActionText.AddTextObject(FindInSimpleView<TextMeshProUGUI>("Current Action"));
        }

        private void SetDetailedView()
        {
            _detailedView = GameObject.transform.Find("Detailed").gameObject;
            _detailedView.SetActive(false);
            
            _actionMenuList = FindInDetailedView<MenuList>("Action List");
            
            _detailedCurrentActionText.AddTextObject(FindInDetailedView<TextMeshProUGUI>("CurrentAction"));

            FindInDetailedView<TextMeshProUGUI>("Detailed Name").text = _character.Name;
            FindInDetailedView<TextMeshProUGUI>("Class").text = _character.CharacterClass.GetTraitDetails();
            FindInDetailedView<TextMeshProUGUI>("Trait").text = _character.CharacterTrait.GetTraitDetails();
            FindInDetailedView<TextMeshProUGUI>("Weight").text = "Weight: " + _character.SurvivalAttributes.Weight + " (requires " + ((int)_character.SurvivalAttributes.Weight + 5) + "fuel)";
            
            _gearContainer = Helper.FindChildWithName(_detailedView, "Gear");
            WeaponGearUi = FindInDetailedView<UIGearController>("Weapon");
            ArmourGearUi = FindInDetailedView<UIGearController>("Armour");
            AccessoryGearUi = FindInDetailedView<UIGearController>("Accessory");
        }
        
        public void UpdateActionUi()
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
                Helper.SetNavigation(weaponGearUiButton, actionUi.GetNavigationButton(),
                    Direction.Right);
                Helper.SetNavigation(weaponGearUiButton, actionUi.GetNavigationButton(),
                    Direction.Right);
                Helper.SetNavigation(weaponGearUiButton, actionUi.GetNavigationButton(),
                    Direction.Right);
            }
        }
        
        public CharacterView(Player character)
        {
            _character = character; 
            GameObject = _character.GetGameObject();
            GameObject.SetActive(true);
            SetSimpleView();
            SetDetailedView();
            BindUi();
            WorldState.RegisterMinuteEvent(delegate
            {
                string currentActionString = _character?.States?.GetCurrentState()?.Name + " " + _character?.States?.GetCurrentState()?.GetCostAsString();
                _currentActionText.Value(currentActionString);
                _detailedCurrentActionText.Value(currentActionString);
            });
            SwitchToSimpleView();
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