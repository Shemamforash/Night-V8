using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Combat;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.ReactiveUI;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
    public class Character : StateMachine
    {
        public CharacterUI CharacterUi;
        public string Name;
        public float StarvationTolerance, DehydrationTolerance;
        public MyFloat Strength = new MyFloat();
        public MyFloat Intelligence = new MyFloat();
        public MyFloat Endurance = new MyFloat();
        public MyFloat Stability = new MyFloat();
        public MyFloat Starvation = new MyFloat();
        public MyFloat Dehydration = new MyFloat();
        public MyFloat Hunger, Thirst = new MyFloat();
        public WeightCategory Weight;
        public float Sight;
        public Traits.Trait CharacterClass, CharacterTrait;

        private MyString _weaponName = new MyString();

        private MyFloat _weaponDamage = new MyFloat();
        private MyFloat _weaponFireRate = new MyFloat();
        private MyFloat _weaponReloadSpeed = new MyFloat();
        private MyFloat _weaponCapacity = new MyFloat();
        private MyFloat _weaponHandling = new MyFloat();
        private MyFloat _weaponCriticalChance = new MyFloat();
        private MyFloat _weaponAccuracy = new MyFloat();

        public enum WeightCategory
        {
            VeryLight,
            Light,
            Medium,
            Heavy,
            VeryHeavy
        }

        private GameObject actionButtonPrefab;
        private Weapon _weapon;

        public void SetWeapon(Weapon weapon)
        {
            _weapon = weapon;
            _weaponName.Text = _weapon.GetName();
            _weaponDamage.Val = _weapon.Damage;
            _weaponAccuracy.Val = _weapon.Accuracy;
            _weaponCapacity.Val = _weapon.Capacity;
            _weaponCriticalChance.Val = _weapon.CriticalChance;
            _weaponFireRate.Val = _weapon.FireRate;
            _weaponHandling.Val = _weapon.Handling;
            _weaponReloadSpeed.Val = _weapon.ReloadSpeed;
        }

        public Weapon GetWeapon()
        {
            return _weapon;
        }

        public void Initialise(string name, Traits.Trait classCharacter, Traits.Trait characterTrait, WeightCategory weight)
        {
            Name = name;
            CharacterClass = classCharacter;
            CharacterTrait = characterTrait;
            Weight = weight;
            actionButtonPrefab = Resources.Load("Prefabs/Action Button") as GameObject;
            SetCharacterUi(gameObject);
            AddState(new FindResources(this));
            AddState(new EnterCombat(this));
            AddState(new Sleep(this));
            AddState(new Idle(this));
            SetDefaultState("Idle");
            UpdateActionUi();
        }

        private void UpdateActionUi()
        {
            List<BaseCharacterAction> _availableActions = StatesAsList().Cast<BaseCharacterAction>().ToList();
            for (int i = 0; i < _availableActions.Count; ++i)
            {
                BaseCharacterAction a = _availableActions[i];
                GameObject newActionButton = Instantiate(actionButtonPrefab);
                a.ActionButtonGameObject = newActionButton;
                newActionButton.transform.SetParent(CharacterUi.actionScrollContent.transform);
                newActionButton.transform.Find("Text").GetComponent<Text>().text = a.Name();
                Button currentButton = newActionButton.GetComponent<Button>();
                currentButton.GetComponent<Button>().onClick.AddListener(() => NavigateToState(a.Name()));

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
                    GameObject previousActionButton = _availableActions[i - 1].ActionButtonGameObject;
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
            CharacterUi.ClassTraitText.text = CharacterTrait.Name + " " + CharacterClass.Name;
            CharacterUi.DetailedClassText.text = CharacterClass.GetTraitDetails();
            CharacterUi.DetailedTraitText.text = CharacterTrait.GetTraitDetails();
            CharacterUi.WeightText.text = "Weight: " + Weight + " (requires " + ((int) Weight + 5) + " fuel)";
            CharacterUi.CurrentActionText.SetFormattingFunction(f => GetCurrentState().Name() + " " + ((BaseCharacterAction)GetCurrentState()).GetCostAsString());

            _weaponName = new MyString("");
            _weaponName.AddLinkedText(CharacterUi.WeaponNameTextSimple);
            _weaponName.AddLinkedText(CharacterUi.WeaponNameTextDetailed);

            _weaponDamage.AddLinkedText(CharacterUi.WeaponDamageText);
            _weaponAccuracy.AddLinkedText(CharacterUi.WeaponAccuracyText);
            _weaponFireRate.AddLinkedText(CharacterUi.WeaponFireRateText);
            _weaponReloadSpeed.AddLinkedText(CharacterUi.WeaponReloadSpeedText);
            _weaponCapacity.AddLinkedText(CharacterUi.WeaponCapacityText);
            _weaponCriticalChance.AddLinkedText(CharacterUi.WeaponCriticalChanceText);
            _weaponHandling.AddLinkedText(CharacterUi.WeaponHandlingText);
        }

        public string GetConditions()
        {
            string conditions = "";
            conditions += GetThirstStatus() + "(" + Mathf.Round(Thirst.Val / 1.2f) / 10f + " litres/hr)\n";
            conditions += GetHungerStatus() + "(" + Mathf.Round(Hunger.Val / 1.2f) / 10f + " meals/hr)";
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
            return GetHungerStatus(Starvation.Val);
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
            return GetThirstStatus(Dehydration.Val);
        }

        public CharacterUI GetCharacterUi()
        {
            return CharacterUi;
        }

        public void Drink()
        {
            float consumed = Home.Inventory().DecrementResource("Water", 0.25f);
            Dehydration.Val -= consumed;
        }

        public void Eat()
        {
            float consumed = Home.Inventory().DecrementResource("Food", 1);
            Starvation.Val -= consumed;
        }
    }
}