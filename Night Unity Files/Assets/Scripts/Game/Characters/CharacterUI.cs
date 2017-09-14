using Game.Characters;
using SamsHelper;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
    public class CharacterUI
    {
        public GameObject GameObject, SimpleView, DetailedView;
        public Button EatButton;
        public Button DrinkButton;
        public Button CollapseCharacterButton;
        public GameObject ActionScrollContent, WeaponCard;

        public TextMeshProUGUI CurrentActionText, DetailedCurrentActionText;
        public TextMeshProUGUI ConditionsText;
        public TextMeshProUGUI ThirstText, HungerText, StrengthText, IntelligenceText, EnduranceText, StabilityText;
        public TextMeshProUGUI StrengthTextDetail, IntelligenceTextDetail, EnduranceTextDetail, StabilityTextDetail;

        public TextMeshProUGUI NameText, ClassTraitText, DetailedClassText, DetailedTraitText;
        public TextMeshProUGUI WeightText;

        public TextMeshProUGUI WeaponNameTextDetailed,
            WeaponNameTextSimple,
            WeaponModifier1Text,
            WeaponModifier2Text;

        public TextMeshProUGUI WeaponDamageText,
            WeaponFireRateText,
            WeaponReloadSpeedText,
            WeaponCapacityText,
            WeaponHandlingText,
            WeaponCriticalChanceText,
            WeaponAccuracyText;

        public CharacterUI(GameObject gameObject)
        {
            GameObject = gameObject;
            gameObject.SetActive(true);
            SimpleView = GameObject.transform.Find("Simple").gameObject;
            SimpleView.SetActive(true);
            DetailedView = GameObject.transform.Find("Detailed").gameObject;
            DetailedView.SetActive(false);

            ActionScrollContent = Helper.FindChildWithName(gameObject.transform, "Content").gameObject;
            CollapseCharacterButton = FindInDetailedView<Button>("Back Button");
            CollapseCharacterButton.onClick.AddListener(CharacterManager.ExitCharacter);

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
            WeaponCard = Helper.FindChildWithName(DetailedView.transform, "Weapon Card").gameObject;
            ConditionsText = FindInDetailedView<TextMeshProUGUI>("Conditions");

            StrengthTextDetail = FindInDetailedView<TextMeshProUGUI>("Strength");
            IntelligenceTextDetail = FindInDetailedView<TextMeshProUGUI>("Intelligence");
            EnduranceTextDetail = FindInDetailedView<TextMeshProUGUI>("Endurance");
            StabilityTextDetail = FindInDetailedView<TextMeshProUGUI>("Stability");

            WeaponNameTextSimple = FindInSimpleView<TextMeshProUGUI>("Weapon Name");
            WeaponNameTextDetailed = FindInDetailedView<TextMeshProUGUI>("Weapon Name");
            WeaponModifier1Text = FindInDetailedView<TextMeshProUGUI>("Primary Modifier");
            WeaponModifier2Text = FindInDetailedView<TextMeshProUGUI>("Secondary Modifier");

            WeaponDamageText = FindInDetailedView<TextMeshProUGUI>("Damage");
            WeaponFireRateText = FindInDetailedView<TextMeshProUGUI>("Fire Rate");
            WeaponReloadSpeedText = FindInDetailedView<TextMeshProUGUI>("Reload Speed");
            WeaponCapacityText = FindInDetailedView<TextMeshProUGUI>("Capacity");
            WeaponHandlingText = FindInDetailedView<TextMeshProUGUI>("Handling");
            WeaponCriticalChanceText = FindInDetailedView<TextMeshProUGUI>("Critical Chance");
            WeaponAccuracyText = FindInDetailedView<TextMeshProUGUI>("Accuracy");
        }

        public T FindInSimpleView<T>(string name)
        {
            return Helper.FindChildWithName<T>(SimpleView, name);
        }

        public T FindInDetailedView<T>(string name)
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