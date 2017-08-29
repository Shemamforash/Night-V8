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

        public ReactiveText<float> CurrentActionText, DetailedCurrentActionText;
        public ReactiveText<string> ConditionsText;
        public ReactiveText<float> ThirstText, HungerText, StrengthText, IntelligenceText, EnduranceText, StabilityText;
        public ReactiveText<float> StrengthTextDetail, IntelligenceTextDetail, EnduranceTextDetail, StabilityTextDetail;

        public Text NameText, ClassTraitText, DetailedClassText, DetailedTraitText;
        public Text WeightText;

        public ReactiveText<string> WeaponNameTextDetailed,
            WeaponNameTextSimple,
            WeaponModifier1Text,
            WeaponModifier2Text;

        public ReactiveText<float> WeaponDamageText,
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

            ThirstText = new ReactiveText<float>(FindInSimpleView<Text>("Thirst"));
            HungerText = new ReactiveText<float>(FindInSimpleView<Text>("Hunger"));
            StrengthText = new ReactiveText<float>(FindInSimpleView<Text>("Strength"), f => f + " str");
            IntelligenceText = new ReactiveText<float>(FindInSimpleView<Text>("Intelligence"), f => f + " int");
            EnduranceText = new ReactiveText<float>(FindInSimpleView<Text>("Endurance"), f => f + " end");
            StabilityText = new ReactiveText<float>(FindInSimpleView<Text>("Stability"), f => f + " stab");

            NameText = FindInSimpleView<Text>("Simple Name");
            ClassTraitText = FindInSimpleView<Text>("ClassTrait");
            CurrentActionText = new ReactiveText<float>(FindInSimpleView<Text>("Current Action"));

            DetailedCurrentActionText = new ReactiveText<float>(FindInDetailedView<Text>("CurrentAction"));
            DetailedClassText = FindInDetailedView<Text>("Class");
            DetailedTraitText = FindInDetailedView<Text>("Trait");
            WeightText = FindInDetailedView<Text>("Weight");

            EatButton = FindInDetailedView<Button>("Eat Button");
            DrinkButton = FindInDetailedView<Button>("Drink Button");
            WeaponCard = Helper.FindChildWithName(DetailedView.transform, "Weapon Card").gameObject;
            ConditionsText = new ReactiveText<string>(FindInDetailedView<Text>("Conditions"));

            StrengthTextDetail = new ReactiveText<float>(FindInDetailedView<Text>("Strength"));
            IntelligenceTextDetail = new ReactiveText<float>(FindInDetailedView<Text>("Intelligence"));
            EnduranceTextDetail = new ReactiveText<float>(FindInDetailedView<Text>("Endurance"));
            StabilityTextDetail = new ReactiveText<float>(FindInDetailedView<Text>("Stability"));

            WeaponNameTextSimple = new ReactiveText<string>(FindInSimpleView<Text>("Weapon Name"));
            WeaponNameTextDetailed = new ReactiveText<string>(FindInDetailedView<Text>("Weapon Name"));
            WeaponModifier1Text = new ReactiveText<string>(FindInDetailedView<Text>("Primary Modifier"));
            WeaponModifier2Text = new ReactiveText<string>(FindInDetailedView<Text>("Secondary Modifier"));

            WeaponDamageText =
                new ReactiveText<float>(FindInDetailedView<Text>("Damage"), f => Helper.Round(f, 2) + "dam");
            WeaponFireRateText = new ReactiveText<float>(FindInDetailedView<Text>("Fire Rate"),
                f => Helper.Round(f, 2) + "rnds/s");
            WeaponReloadSpeedText = new ReactiveText<float>(FindInDetailedView<Text>("Reload Speed"),
                f => Helper.Round(f, 2) + "s rel");
            WeaponCapacityText =
                new ReactiveText<float>(FindInDetailedView<Text>("Capacity"), f => Helper.Round(f, 0) + " cap");
            WeaponHandlingText =
                new ReactiveText<float>(FindInDetailedView<Text>("Handling"), f => Helper.Round(f, 2) + "% hand");
            WeaponCriticalChanceText =
                new ReactiveText<float>(FindInDetailedView<Text>("Critical Chance"),
                    f => Helper.Round(f, 2) + "% crit");
            WeaponAccuracyText =
                new ReactiveText<float>(FindInDetailedView<Text>("Accuracy"), f => Helper.Round(f, 2) + "% acc");
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