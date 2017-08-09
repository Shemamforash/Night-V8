using Facilitating.UI.GameOnly;
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

        public CharacterUI(GameObject gameObject)
        {
            GameObject = gameObject;
            gameObject.SetActive(true);
            SimpleView = GameObject.transform.Find("Simple").gameObject;
            SimpleView.SetActive(true);
            DetailedView = GameObject.transform.Find("Detailed").gameObject;
            DetailedView.SetActive(false);

            actionScrollContent = Helper.FindChildWithName(gameObject, "Content").gameObject;
            CollapseCharacterButton = FindInDetailedView<Button>("Back Button");
            CollapseCharacterButton.onClick.AddListener(
                gameObject.transform.parent.GetComponent<CharacterSelect>().ExitCharacter);

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
            WeaponCard = Helper.FindChildWithName(DetailedView, "Weapon Card").gameObject;
            ConditionsText = FindInDetailedView<Text>("Conditions");

            StrengthTextDetail = FindInDetailedView<Text>("Strength");
            IntelligenceTextDetail = FindInDetailedView<Text>("Intelligence");
            EnduranceTextDetail = FindInDetailedView<Text>("Endurance");
            StabilityTextDetail = FindInDetailedView<Text>("Stability");
        }

        public T FindInSimpleView<T>(string name)
        {
            return Helper.FindChildWithName(SimpleView, name).GetComponent<T>();
        }

        public T FindInDetailedView<T>(string name)
        {
            return Helper.FindChildWithName(DetailedView, name).GetComponent<T>();
        }
    }
}