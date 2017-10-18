using System;
using System.Collections.Generic;
using System.Linq;
using Facilitating.MenuNavigation;
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
        private readonly DesolationCharacter _character;
        public readonly GameObject GameObject;
        public GameObject SimpleView;
        private GameObject _detailedView;
        private GameObject _gearContainer;
        public Button CollapseCharacterButton;

        private readonly ValueTextLink<string> _currentActionText = new ValueTextLink<string>();
        private readonly ValueTextLink<string> _detailedCurrentActionText = new ValueTextLink<string>();

        private TextMeshProUGUI _thirstText, _hungerText;

        public GearUi WeaponGearUi, ArmourGearUi, AccessoryGearUi;

        private MenuList _actionMenuList;

        public void SetActionListActive(bool active)
        {
            _actionMenuList.gameObject.SetActive(active);
            _detailedCurrentActionText.SetEnabled(!active);
        }
        
        public class GearUi
        {
            private readonly TextMeshProUGUI _name, _summary;
            private readonly GearSubtype _gearType;
            public readonly EnhancedButton GearButton;

            public GearUi(GearSubtype gearType, GameObject gearContainer, Character c)
            {
                gearContainer = Helper.FindChildWithName(gearContainer, gearType.ToString());
                _gearType = gearType;
                GearButton = gearContainer.GetComponent<EnhancedButton>();
                Helper.FindChildWithName<TextMeshProUGUI>(gearContainer, "Slot").text = _gearType.ToString();
                _name = Helper.FindChildWithName<TextMeshProUGUI>(gearContainer, "Name");
                _summary = Helper.FindChildWithName<TextMeshProUGUI>(gearContainer, "Summary");
                GearButton.AddOnClick(() => OpenEquipMenu(c));
            }

            private void OpenEquipMenu(Character character)
            {
                Popup popup = new Popup("Equip " + _gearType);
                List<MyGameObject> availableGear = new List<MyGameObject>();
                List<MyGameObject> allGear = new List<MyGameObject>();
                allGear.AddRange(character.Inventory().Contents());
                allGear.AddRange(WorldState.HomeInventory().Contents());
                foreach (MyGameObject item in allGear)
                {
                    GearItem gear = item as GearItem;
                    if (gear != null && gear.GetGearType() == _gearType && !gear.Equipped)
                    {
                        availableGear.Add(gear);
                    }
                }
                popup.AddList(availableGear, g => character.Equip((GearItem) g), true, true, true);
                popup.AddBackButton();
            }

            public void Update(GearItem item)
            {
                if (item != null)
                {
                    _name.text = item.Name;
                    _summary.text = item.GetSummary();
                }
                else
                {
                    _name.text = "- Not Equipped -";
                    _summary.text = "--";
                }
            }
        }

        private void BindUi()
        {
            DesolationCharacterAttributes attributes = _character.Attributes;
            BindUiToAttribute(attributes.Strength,  "Strength");
            BindUiToAttribute(attributes.Endurance, "Endurance");
            BindUiToAttribute(attributes.Stability, "Stability");
            BindUiToAttribute(attributes.Intelligence, "Intelligence");
            attributes.Hunger.AddOnValueChange(f => _hungerText.text = attributes.GetHungerStatus());
            attributes.Thirst.AddOnValueChange(f => _thirstText.text = attributes.GetThirstStatus());
        }
        
        private void BindUiToAttribute(CharacterAttribute a, string attributeName)
        {
            TextMeshProUGUI simpleText = FindInSimpleView<TextMeshProUGUI>(attributeName);
            TextMeshProUGUI detailText = FindInDetailedView<TextMeshProUGUI>(attributeName);
            a.AddOnValueChange(delegate(MyValue f)
            {
                int calculatedValue = (int) ((CharacterAttribute) f).GetCalculatedValue();
                simpleText.text = calculatedValue.ToString();
                detailText.text = calculatedValue + "/" + a.Max;
            });
        }
        
        private void SetSimpleView()
        {
            SimpleView = GameObject.transform.Find("Simple").gameObject;
            SimpleView.SetActive(true);
            _thirstText = FindInSimpleView<TextMeshProUGUI>("Thirst");
            _hungerText = FindInSimpleView<TextMeshProUGUI>("Hunger");

            FindInSimpleView<TextMeshProUGUI>("Simple Name").text = _character.Name;
            FindInSimpleView<TextMeshProUGUI>("ClassTrait").text = _character.CharacterTrait.Name + " " + _character.CharacterClass.Name;
            
            _currentActionText.AddTextObject(FindInSimpleView<TextMeshProUGUI>("Current Action"));
        }

        private void SetDetailedView()
        {
            _detailedView = GameObject.transform.Find("Detailed").gameObject;
            _detailedView.SetActive(false);
            
            _actionMenuList = FindInDetailedView<MenuList>("Action List");
            CollapseCharacterButton = FindInDetailedView<Button>("Back Button");
            CollapseCharacterButton.onClick.AddListener(DesolationCharacterManager.ExitCharacter);
            
            _detailedCurrentActionText.AddTextObject(FindInDetailedView<TextMeshProUGUI>("CurrentAction"));

            FindInDetailedView<TextMeshProUGUI>("Detailed Name").text = _character.Name;
            FindInDetailedView<TextMeshProUGUI>("Class").text = _character.CharacterClass.GetTraitDetails();
            FindInDetailedView<TextMeshProUGUI>("Trait").text = _character.CharacterTrait.GetTraitDetails();
            FindInDetailedView<TextMeshProUGUI>("Weight").text = "Weight: " + _character.Attributes.Weight + " (requires " + ((int)_character.Attributes.Weight + 5) + "fuel)";
            
            _character.Conditions.Thoughts.AddTextObject(FindInDetailedView<TextMeshProUGUI>("Conditions"));

            _gearContainer = Helper.FindChildWithName(_detailedView, "Gear");
            WeaponGearUi = new GearUi(GearSubtype.Weapon, _gearContainer, _character);
            ArmourGearUi = new GearUi(GearSubtype.Armour, _gearContainer, _character);
            AccessoryGearUi = new GearUi(GearSubtype.Accessory, _gearContainer, _character);
        }
        
        public void UpdateActionUi()
        {
            List<BaseCharacterAction> availableActions = _character.StatesAsList(false).Cast<BaseCharacterAction>().ToList();
            _actionMenuList.SetItems(new List<MyGameObject>(availableActions));

            List<InventoryUi> actionUiList = _actionMenuList.GetItems();
            for (int i = 0; i < actionUiList.Count; ++i)
            {
                InventoryUi actionUi = actionUiList[i];

                Helper.SetNavigation(actionUi.GetNavigationButton(), WeaponGearUi.GearButton.gameObject, Direction.Left);
                if (i == availableActions.Count - 1)
                {
                    Helper.SetReciprocalNavigation(actionUi.GetNavigationButton(), CollapseCharacterButton.gameObject);
                }

                else if (i == 0)
                {
                    Helper.SetNavigation(WeaponGearUi.GearButton.gameObject, actionUi.GetNavigationButton(),
                        Direction.Right);
                    Helper.SetNavigation(ArmourGearUi.GearButton.gameObject, actionUi.GetNavigationButton(),
                        Direction.Right);
                    Helper.SetNavigation(AccessoryGearUi.GearButton.gameObject, actionUi.GetNavigationButton(),
                        Direction.Right);
                }
            }
        }
        
        public CharacterView(DesolationCharacter character)
        {
            _character = character; 
            GameObject = _character.GetGameObject();
            GameObject.SetActive(true);
            SetSimpleView();
            SetDetailedView();
            BindUi();
            WorldState.RegisterMinuteEvent(delegate
            {
                string currentActionString = _character.ActionStates.GetCurrentState().Name + " " + ((BaseCharacterAction) _character.ActionStates.GetCurrentState()).GetCostAsString();
                _currentActionText.Value(currentActionString);
                _detailedCurrentActionText.Value(currentActionString);
            });
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
            CollapseCharacterButton.Select();
        }

        public void SwitchToSimpleView()
        {
            GameObject.GetComponent<LayoutElement>().preferredHeight = 50;
            _detailedView.SetActive(false);
            SimpleView.SetActive(true);
            SimpleView.GetComponent<Button>().Select();
        }
    }
}