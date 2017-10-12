using System;
using System.Collections.Generic;
using Facilitating.MenuNavigation;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Characters
{
    public class CharacterUiDetailed
    {
        public readonly Character _character;
        public readonly GameObject GameObject, SimpleView, DetailedView, GearContainer;
        public readonly Button CollapseCharacterButton;

        public readonly TextMeshProUGUI CurrentActionText, DetailedCurrentActionText;
        public readonly TextMeshProUGUI ConditionsText;
        public readonly TextMeshProUGUI ThirstText, HungerText, StrengthText, IntelligenceText, EnduranceText, StabilityText;
        public readonly TextMeshProUGUI StrengthTextDetail, IntelligenceTextDetail, EnduranceTextDetail, StabilityTextDetail;

        public readonly TextMeshProUGUI NameText, ClassTraitText, DetailedClassText, DetailedTraitText;
        public readonly TextMeshProUGUI WeightText;
        public readonly GearUi WeaponGearUi, ArmourGearUi, AccessoryGearUi;

        public readonly MenuList ActionMenuList;

        public class GearUi
        {
            private readonly TextMeshProUGUI _name, _summary;
            private GearSubtype _gearType;
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
                allGear.AddRange(character.Inventory.Contents());
                allGear.AddRange(WorldState.HomeInventory.Contents());
                foreach (MyGameObject item in allGear)
                {
                    GearItem gear = item as GearItem;
                    if (gear != null && gear.GetGearType() == _gearType && !gear.Equipped)
                    {
                        availableGear.Add(gear);
                    }
                }
                popup.AddList(availableGear, g => character.Equip((GearItem) g), false, true);
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
        
//        public readonly TextMeshProUGUI WeaponNameTextDetailed,
//            WeaponNameTextSimple,
//            WeaponModifier1Text,
//            WeaponModifier2Text;

//        public readonly TextMeshProUGUI WeaponDamageText,
//            WeaponFireRateText,
//            WeaponReloadSpeedText,
//            WeaponCapacityText,
//            WeaponHandlingText,
//            WeaponCriticalChanceText,
//            WeaponAccuracyText;

        public CharacterUiDetailed(Character character)
        {
            _character = character; 
            GameObject = _character.GameObject;
            GameObject.SetActive(true);
            SimpleView = GameObject.transform.Find("Simple").gameObject;
            SimpleView.SetActive(true);
            DetailedView = GameObject.transform.Find("Detailed").gameObject;
            DetailedView.SetActive(false);

            ActionMenuList = FindInDetailedView<MenuList>("Action List");
            CollapseCharacterButton = FindInDetailedView<Button>("Back Button");
            CollapseCharacterButton.onClick.AddListener(DesolationCharacterManager.ExitCharacter);

            ThirstText = FindInSimpleView<TextMeshProUGUI>("Thirst");
            HungerText = FindInSimpleView<TextMeshProUGUI>("Hunger");
            StrengthText = FindInSimpleView<TextMeshProUGUI>("Strength"); //, f => f + " str");
            IntelligenceText = FindInSimpleView<TextMeshProUGUI>("Intelligence"); //, f => f + " int");
            EnduranceText = FindInSimpleView<TextMeshProUGUI>("Endurance"); //, f => f + " end");
            StabilityText = FindInSimpleView<TextMeshProUGUI>("Stability"); //, f => f + " stab");

            NameText = FindInSimpleView<TextMeshProUGUI>("Simple Name");
            ClassTraitText = FindInSimpleView<TextMeshProUGUI>("ClassTrait");
            CurrentActionText = FindInSimpleView<TextMeshProUGUI>("Current Action");

            DetailedCurrentActionText = FindInDetailedView<TextMeshProUGUI>("CurrentAction");
            DetailedClassText = FindInDetailedView<TextMeshProUGUI>("Class");
            DetailedTraitText = FindInDetailedView<TextMeshProUGUI>("Trait");
            WeightText = FindInDetailedView<TextMeshProUGUI>("Weight");

//            WeaponCard = Helper.FindChildWithName(DetailedView.transform, "Weapon Card").gameObject;
            ConditionsText = FindInDetailedView<TextMeshProUGUI>("Conditions");

            StrengthTextDetail = FindInDetailedView<TextMeshProUGUI>("Strength");
            IntelligenceTextDetail = FindInDetailedView<TextMeshProUGUI>("Intelligence");
            EnduranceTextDetail = FindInDetailedView<TextMeshProUGUI>("Endurance");
            StabilityTextDetail = FindInDetailedView<TextMeshProUGUI>("Stability");

            GearContainer = Helper.FindChildWithName(DetailedView, "Gear");
            WeaponGearUi = new GearUi(GearSubtype.Weapon, GearContainer, _character);
            ArmourGearUi = new GearUi(GearSubtype.Armour, GearContainer, _character);
            AccessoryGearUi = new GearUi(GearSubtype.Accessory, GearContainer, _character);

//            WeaponNameTextSimple = FindInSimpleView<TextMeshProUGUI>("Weapon Name");
//            WeaponNameTextDetailed = FindInDetailedView<TextMeshProUGUI>("Weapon Name");
//            WeaponModifier1Text = FindInDetailedView<TextMeshProUGUI>("Primary Modifier");
//            WeaponModifier2Text = FindInDetailedView<TextMeshProUGUI>("Secondary Modifier");

//            WeaponDamageText = FindInDetailedView<TextMeshProUGUI>("Damage");
//            WeaponFireRateText = FindInDetailedView<TextMeshProUGUI>("Fire Rate");
//            WeaponReloadSpeedText = FindInDetailedView<TextMeshProUGUI>("Reload Speed");
//            WeaponCapacityText = FindInDetailedView<TextMeshProUGUI>("Capacity");
//            WeaponHandlingText = FindInDetailedView<TextMeshProUGUI>("Handling");
//            WeaponCriticalChanceText = FindInDetailedView<TextMeshProUGUI>("Critical Chance");
//            WeaponAccuracyText = FindInDetailedView<TextMeshProUGUI>("Accuracy");
        }

        private T FindInSimpleView<T>(string name)
        {
            return Helper.FindChildWithName<T>(SimpleView, name);
        }

        private T FindInDetailedView<T>(string name)
        {
            return Helper.FindChildWithName<T>(DetailedView, name);
        }

        public void SwitchToDetailedView()
        {
            GameObject.GetComponent<LayoutElement>().preferredHeight = 200;
            DetailedView.SetActive(true);
            SimpleView.SetActive(false);
            CollapseCharacterButton.Select();
        }

        public void SwitchToSimpleView()
        {
            GameObject.GetComponent<LayoutElement>().preferredHeight = 50;
            DetailedView.SetActive(false);
            SimpleView.SetActive(true);
            SimpleView.GetComponent<Button>().Select();
        }
    }
}