using System.Collections.Generic;
using Facilitating.MenuNavigation;
using Game.Characters;
using Game.Combat;
using Game.Misc;
using Game.World;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace Characters
{
    public class Character
    {
        public CharacterUI CharacterUi;
        public string Name;
        public MyFloat Strength;
        public MyFloat Intelligence;
        public MyFloat Endurance;
        public MyFloat Stability;
        public float StarvationTolerance, DehydrationTolerance;
        public MyFloat Starvation;
        public MyFloat Dehydration;
        public MyFloat Hunger, Thirst;
        public WeightCategory Weight;
        public float Sight;
        public Traits.Trait PrimaryTrait, SecondaryTrait;

        private MyString WeaponName;

        private MyFloat WeaponDamage,
            WeaponFireRate,
            WeaponReloadSpeed,
            WeaponCapacity,
            WeaponHandling,
            WeaponCriticalChance,
            WeaponAccuracy;

        private ClassCharacter _characterClass;

        public enum WeightCategory
        {
            VeryLight,
            Light,
            Medium,
            Heavy,
            VeryHeavy
        };

        private List<CharacterAction> _availableActions = new List<CharacterAction>();
        private GameObject actionButtonPrefab;
        private Weapon _weapon;

        public void SetWeapon(Weapon weapon)
        {
            _weapon = weapon;
            WeaponName.SetText(_weapon.GetName());
            WeaponDamage.Value = _weapon.Damage;
            WeaponAccuracy.Value = _weapon.Accuracy;
            WeaponCapacity.Value = _weapon.Capacity;
            WeaponCriticalChance.Value = _weapon.CriticalChance;
            WeaponFireRate.Value = _weapon.FireRate;
            WeaponHandling.Value = _weapon.Handling;
            WeaponReloadSpeed.Value = _weapon.ReloadSpeed;
        }

        public Weapon GetWeapon()
        {
            return _weapon;
        }

        public Character(string name, ClassCharacter classCharacter, WeightCategory weight, Traits.Trait secondaryTrait)
        {
            Name = name;
            _characterClass = classCharacter;
            Weight = weight;
            PrimaryTrait = Traits.FindTrait(_characterClass.ClassTrait());
            SecondaryTrait = secondaryTrait;

            GameObject characterUi = GameObject.Instantiate(Resources.Load("Prefabs/Character Template") as GameObject);
            characterUi.transform.SetParent(GameObject.Find("Characters").transform);
            SetCharacterUi(characterUi);

            _availableActions.Add(new CharacterAction.FindResources(this));
            _availableActions.Add(new CharacterAction.Hunt(this));
            actionButtonPrefab = Resources.Load("Prefabs/Action Button") as GameObject;

            UpdateActionUi();
        }

        private void UpdateActionUi()
        {
            for (int i = 0; i < _availableActions.Count; ++i)
            {
                CharacterAction a = _availableActions[i];
                GameObject newActionButton = GameObject.Instantiate(actionButtonPrefab);
                a.ActionObject = newActionButton;
                newActionButton.transform.SetParent(CharacterUi.actionScrollContent.transform);
                newActionButton.transform.Find("Text").GetComponent<Text>().text = a.GetActionName();
                Button currentButton = newActionButton.GetComponent<Button>();

                if (!a.IsDurationFixed())
                {
                    currentButton.GetComponent<Button>().onClick
                        .AddListener(() => GameMenuNavigator.MenuNavigator.ShowActionDurationMenu(a));
                }
                else
                {
                    currentButton.GetComponent<Button>().onClick
                        .AddListener(() => a.InitialiseAction());
                }

                Helper.SetNavigation(newActionButton, CharacterUi.WeaponCard, Helper.NavigationDirections.Left);
                if (i == _availableActions.Count - 1)
                {
                    Helper.SetNavigation(newActionButton, CharacterUi.CollapseCharacterButton.gameObject,
                        Helper.NavigationDirections.Down);
                    Helper.SetNavigation(CharacterUi.CollapseCharacterButton.gameObject, newActionButton,
                        Helper.NavigationDirections.Up);
                }

                if (i > 0)
                {
                    GameObject previousActionButton = _availableActions[i - 1].ActionObject;
                    Helper.SetNavigation(newActionButton, previousActionButton, Helper.NavigationDirections.Up);
                    Helper.SetNavigation(previousActionButton, newActionButton, Helper.NavigationDirections.Down);
                }
                else if (i == 0)
                {
                    Helper.SetNavigation(CharacterUi.WeaponCard.gameObject, newActionButton,
                        Helper.NavigationDirections.Right);
                }
            }
        }

        private void SetCharacterUi(GameObject g)
        {
            CharacterUi = new CharacterUI(g);
            CharacterUi.EatButton.onClick.AddListener(Eat);
            CharacterUi.DrinkButton.onClick.AddListener(Drink);

            CharacterUi.NameText.text = Name;
            CharacterUi.ClassTraitText.text = _characterClass.ClassName() + " " + SecondaryTrait.Name;
            CharacterUi.DetailedClassText.text = PrimaryTrait.GetTraitDetails();
            CharacterUi.DetailedTraitText.text = SecondaryTrait.GetTraitDetails();
            CharacterUi.WeightText.text = "Weight: " + Weight + " (requires " + ((int) Weight + 5) + " fuel)";

            WeaponName = new MyString("",
                new List<Text> {CharacterUi.WeaponNameTextSimple, CharacterUi.WeaponNameTextDetailed});
            WeaponDamage = new MyFloat(0, new TextAssociation(CharacterUi.WeaponDamageText, f => f + " dam", true));
            WeaponAccuracy = new MyFloat(0, new TextAssociation(CharacterUi.WeaponAccuracyText, f => f + "% acc", true));
            WeaponFireRate = new MyFloat(0, new TextAssociation(CharacterUi.WeaponFireRateText, f => f + "rnds/s", true));
            WeaponReloadSpeed =
                new MyFloat(0, new TextAssociation(CharacterUi.WeaponReloadSpeedText, f => f + "s rel", true));
            WeaponCapacity = new MyFloat(0, new TextAssociation(CharacterUi.WeaponCapacityText, f => f + " cap", true));
            WeaponCriticalChance =
                new MyFloat(0, new TextAssociation(CharacterUi.WeaponCriticalChanceText, f => f + "% crit", true));
            WeaponHandling = new MyFloat(0, new TextAssociation(CharacterUi.WeaponHandlingText, f => f + "% hand", true));
        }

        public string GetConditions()
        {
            string conditions = "";
            conditions += GetThirstStatus() + "(" + Mathf.Round(Thirst.Value / 1.2f) / 10f + " litres/hr)\n";
            conditions += GetHungerStatus() + "(" + Mathf.Round(Hunger.Value / 1.2f) / 10f + " meals/hr)";
            return conditions;
        }

        public string GetHungerStatus(float f)
        {
            if (f == 0)
            {
                return "Full";
            }
            if (f < Hunger)
            {
                return "Sated";
            }
            if (f < Hunger * 2f)
            {
                return "Hungry";
            }
            if (f < Hunger * 3f)
            {
                Eat();
            }
            if (f >= StarvationTolerance)
            {
                Kill();
            }
            return "Starving";
        }

        public string GetHungerStatus()
        {
            return GetHungerStatus(Starvation.Value);
        }

        public void Kill()
        {
            CharacterManager.RemoveCharacter(this, Name == "Driver");
        }

        public string GetThirstStatus(float f)
        {
            if (f == 0)
            {
                return "Slaked";
            }
            if (f < Thirst)
            {
                return "Quenched";
            }
            if (f < Thirst * 2f)
            {
                return "Thirsty";
            }
            if (f < Thirst * 3f)
            {
                Drink();
            }
            if (f >= DehydrationTolerance)
            {
                Kill();
            }
            return "Parched";
        }

        public string GetThirstStatus()
        {
            return GetThirstStatus(Dehydration.Value);
        }

        public CharacterUI GetCharacterUi()
        {
            return CharacterUi;
        }

        public void Drink()
        {
            float consumed = Home.ConsumeResource(ResourceType.Water, 0.25f);
            Dehydration.Value -= consumed;
        }

        public void Eat()
        {
            float consumed = Home.ConsumeResource(ResourceType.Food, 1);
            Starvation.Value -= consumed;
        }
    }
}