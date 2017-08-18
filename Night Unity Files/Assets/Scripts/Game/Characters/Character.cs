﻿using System.Collections.Generic;
using Facilitating.MenuNavigation;
using Game.Characters;
using Game.Combat;
using Game.World;
using SamsHelper;
using SamsHelper.ReactiveUI;
using UnityEngine;
using UnityEngine.UI;

namespace Characters
{
    public class Character
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
        public Traits.Trait PrimaryTrait, SecondaryTrait;

        private MyString _weaponName = new MyString();

        private MyFloat _weaponDamage = new MyFloat();
        private MyFloat _weaponFireRate = new MyFloat();
        private MyFloat _weaponReloadSpeed = new MyFloat();
        private MyFloat _weaponCapacity = new MyFloat();
        private MyFloat _weaponHandling = new MyFloat();
        private MyFloat _weaponCriticalChance = new MyFloat();
        private MyFloat _weaponAccuracy = new MyFloat();

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
            float consumed = Home.ConsumeResource(ResourceType.Water, 0.25f);
            Dehydration.Val -= consumed;
        }

        public void Eat()
        {
            float consumed = Home.ConsumeResource(ResourceType.Food, 1);
            Starvation.Val -= consumed;
        }
    }
}