using Facilitating.UI.Elements;
using Game.Characters;
using SamsHelper;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.InventoryUI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
    public class CharacterUiDetailed
    {
        public readonly GameObject GameObject, SimpleView, DetailedView, GearContainer;
        public readonly Button EatButton;
        public readonly Button DrinkButton;
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
            public readonly GameObject GearUiObject;
            public readonly TextMeshProUGUI Type, Name, Summary, EquipButtonText;
            public readonly EnhancedButton EquipButton;

            public GearUi(string gearType, GameObject gearContainer)
            {
                GearUiObject = Helper.FindChildWithName(gearContainer, gearType);
                Type = Helper.FindChildWithName<TextMeshProUGUI>(GearUiObject, "Type");
                Name = Helper.FindChildWithName<TextMeshProUGUI>(GearUiObject, "Name");
                Summary = Helper.FindChildWithName<TextMeshProUGUI>(GearUiObject, "Summary");
                EquipButton = Helper.FindChildWithName<EnhancedButton>(GearUiObject, "Equip Button");
                EquipButtonText = Helper.FindChildWithName<TextMeshProUGUI>(EquipButton.gameObject, "Text");
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

        public CharacterUiDetailed(GameObject gameObject)
        {
            GameObject = gameObject;
            gameObject.SetActive(true);
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

            EatButton = FindInDetailedView<Button>("Eat Button");
            DrinkButton = FindInDetailedView<Button>("Drink Button");
//            WeaponCard = Helper.FindChildWithName(DetailedView.transform, "Weapon Card").gameObject;
            ConditionsText = FindInDetailedView<TextMeshProUGUI>("Conditions");

            StrengthTextDetail = FindInDetailedView<TextMeshProUGUI>("Strength");
            IntelligenceTextDetail = FindInDetailedView<TextMeshProUGUI>("Intelligence");
            EnduranceTextDetail = FindInDetailedView<TextMeshProUGUI>("Endurance");
            StabilityTextDetail = FindInDetailedView<TextMeshProUGUI>("Stability");

            GearContainer = Helper.FindChildWithName(DetailedView, "Gear");
            WeaponGearUi = new GearUi("Weapon", GearContainer);
            ArmourGearUi = new GearUi("Armour", GearContainer);
            AccessoryGearUi = new GearUi("Accessory", GearContainer);

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
            DetailedView.SetActive(true);
            SimpleView.SetActive(false);
            EatButton.Select();
        }

        public void SwitchToSimpleView()
        {
            DetailedView.SetActive(false);
            SimpleView.SetActive(true);
            SimpleView.GetComponent<Button>().Select();
        }
    }
}