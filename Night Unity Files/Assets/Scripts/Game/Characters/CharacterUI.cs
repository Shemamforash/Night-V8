using Game.Characters;
using SamsHelper;
using SamsHelper.ReactiveUI;
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

        public Text CurrentActionText, DetailedCurrentActionText;
        public Text ConditionsText;
        public Text ThirstText, HungerText, StrengthText, IntelligenceText, EnduranceText, StabilityText;
        public Text StrengthTextDetail, IntelligenceTextDetail, EnduranceTextDetail, StabilityTextDetail;

        public Text NameText, ClassTraitText, DetailedClassText, DetailedTraitText;
        public Text WeightText;

        public ReactiveText<string> WeaponNameTextDetailed,
            WeaponNameTextSimple,
            WeaponModifier1Text,
            WeaponModifier2Text;

        public Text WeaponDamageText,
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

            ThirstText = FindInSimpleView<Text>("Thirst");
            HungerText = FindInSimpleView<Text>("Hunger");
            StrengthText = FindInSimpleView<Text>("Strength"); //, f => f + " str");
            IntelligenceText = FindInSimpleView<Text>("Intelligence"); //, f => f + " int");
            EnduranceText = FindInSimpleView<Text>("Endurance"); //, f => f + " end");
            StabilityText = FindInSimpleView<Text>("Stability"); //, f => f + " stab");

            NameText = FindInSimpleView<Text>("Simple Name");
            ClassTraitText = FindInSimpleView<Text>("ClassTrait");
            CurrentActionText = FindInSimpleView<Text>("Current Action");

            DetailedCurrentActionText = FindInDetailedView<Text>("CurrentAction");
            DetailedClassText = FindInDetailedView<Text>("Class");
            DetailedTraitText = FindInDetailedView<Text>("Trait");
            WeightText = FindInDetailedView<Text>("Weight");

            EatButton = FindInDetailedView<Button>("Eat Button");
            DrinkButton = FindInDetailedView<Button>("Drink Button");
            WeaponCard = Helper.FindChildWithName(DetailedView.transform, "Weapon Card").gameObject;
            ConditionsText = FindInDetailedView<Text>("Conditions");

            StrengthTextDetail = FindInDetailedView<Text>("Strength");
            IntelligenceTextDetail = FindInDetailedView<Text>("Intelligence");
            EnduranceTextDetail = FindInDetailedView<Text>("Endurance");
            StabilityTextDetail = FindInDetailedView<Text>("Stability");

            WeaponNameTextSimple = new ReactiveText<string>(FindInSimpleView<Text>("Weapon Name"));
            WeaponNameTextDetailed = new ReactiveText<string>(FindInDetailedView<Text>("Weapon Name"));
            WeaponModifier1Text = new ReactiveText<string>(FindInDetailedView<Text>("Primary Modifier"));
            WeaponModifier2Text = new ReactiveText<string>(FindInDetailedView<Text>("Secondary Modifier"));

            WeaponDamageText = FindInDetailedView<Text>("Damage"); //, f => Helper.Round(f, 2) + "dam");
            WeaponFireRateText = FindInDetailedView<Text>("Fire Rate"); //f => Helper.Round(f, 2) + "rnds/s");
            WeaponReloadSpeedText = FindInDetailedView<Text>("Reload Speed"); //f => Helper.Round(f, 2) + "s rel");
            WeaponCapacityText = FindInDetailedView<Text>("Capacity"); //f => Helper.Round(f, 0) + " cap");
            WeaponHandlingText = FindInDetailedView<Text>("Handling"); //f => Helper.Round(f, 2) + "% hand");
            WeaponCriticalChanceText = FindInDetailedView<Text>("Critical Chance"); //,f => Helper.Round(f, 2) + "% crit");
            WeaponAccuracyText = FindInDetailedView<Text>("Accuracy"); //f => Helper.Round(f, 2) + "% acc");
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