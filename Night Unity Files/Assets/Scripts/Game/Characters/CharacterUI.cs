using Game.Characters;
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
        public GameObject actionScrollContent, WeaponCard;
        public Text CurrentActionText;
        public Text ThirstText, HungerText, StrengthText, IntelligenceText, EnduranceText, StabilityText;
        public Text StrengthTextDetail, IntelligenceTextDetail, EnduranceTextDetail, StabilityTextDetail;
        public Text NameText, ClassTraitText, DetailedClassText, DetailedTraitText;
        public Text ConditionsText, WeightText;

        public Text WeaponNameTextDetailed,
            WeaponNameTextSimple,
            WeaponModifier1Text,
            WeaponModifier2Text,
            WeaponDamageText,
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

            actionScrollContent = Helper.FindChildWithName<GameObject>(gameObject, "Content");
            CollapseCharacterButton = FindInDetailedView<Button>("Back Button");
            CollapseCharacterButton.onClick.AddListener(CharacterManager.ExitCharacter);

            ThirstText = FindInSimpleView<Text>("Thirst");
            HungerText = FindInSimpleView<Text>("Hunger");
            StrengthText = FindInSimpleView<Text>("Strength");
            IntelligenceText = FindInSimpleView<Text>("Intelligence");
            EnduranceText = FindInSimpleView<Text>("Endurance");
            StabilityText = FindInSimpleView<Text>("Stability");

            NameText = FindInSimpleView<Text>("Simple Name");
            ClassTraitText = FindInSimpleView<Text>("ClassTrait");
            CurrentActionText = FindInSimpleView<Text>("Current Action");

            DetailedClassText = FindInDetailedView<Text>("Class");
            DetailedTraitText = FindInDetailedView<Text>("Trait");
            WeightText = FindInDetailedView<Text>("Weight");

            EatButton = FindInDetailedView<Button>("Eat Button");
            DrinkButton = FindInDetailedView<Button>("Drink Button");
            WeaponCard = Helper.FindChildWithName<GameObject>(DetailedView, "Weapon Card");
            ConditionsText = FindInDetailedView<Text>("Conditions");

            StrengthTextDetail = FindInDetailedView<Text>("Strength");
            IntelligenceTextDetail = FindInDetailedView<Text>("Intelligence");
            EnduranceTextDetail = FindInDetailedView<Text>("Endurance");
            StabilityTextDetail = FindInDetailedView<Text>("Stability");

            WeaponNameTextSimple = FindInSimpleView<Text>("Weapon Name");
            WeaponNameTextDetailed = FindInDetailedView<Text>("Weapon Name");
            WeaponModifier1Text = FindInDetailedView<Text>("Primary Modifier");
            WeaponModifier2Text = FindInDetailedView<Text>("Secondary Modifier");
            WeaponDamageText = FindInDetailedView<Text>("Damage");
            WeaponFireRateText = FindInDetailedView<Text>("Fire Rate");
            WeaponReloadSpeedText = FindInDetailedView<Text>("Reload Speed");
            WeaponCapacityText = FindInDetailedView<Text>("Capacity");
            WeaponHandlingText = FindInDetailedView<Text>("Handling");
            WeaponCriticalChanceText = FindInDetailedView<Text>("Critical Chance");
            WeaponAccuracyText = FindInDetailedView<Text>("Accuracy");
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