using System;
using System.Collections.Generic;
using System.Linq;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.Combat;
using Game.Combat.Weapons;
using Game.World;
using Game.World.Region;
using Game.World.Time;
using SamsHelper;
using SamsHelper.BaseGameFunctionality;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.CustomTypes;
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
        public MyFloat Hunger = new MyFloat();
        public MyFloat Thirst = new MyFloat();
        public WeightCategory Weight;
        public float Sight;
        public Traits.Trait CharacterClass, CharacterTrait;
        public Region CurrentRegion;

        private MyString _weaponName = new MyString();

        private readonly MyFloat _weaponDamage = new MyFloat();
        private readonly MyFloat _weaponFireRate = new MyFloat();
        private readonly MyFloat _weaponReloadSpeed = new MyFloat();
        private readonly MyFloat _weaponCapacity = new MyFloat();
        private readonly MyFloat _weaponHandling = new MyFloat();
        private readonly MyFloat _weaponCriticalChance = new MyFloat();
        private readonly MyFloat _weaponAccuracy = new MyFloat();

        public DesolationInventory CharacterInventory = new DesolationInventory();

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

        public void AddItemToInventory(InventoryItem item)
        {
            CharacterInventory.AddItem(item);
        }

        public void TakeDamage(float amount)
        {
            Strength.Val -= amount;
            if (Strength.ReachedMin())
            {
                //TODO kill character
            }
        }

        public void Tire(float amount)
        {
            Endurance.Val = Endurance.Val - amount;
            if (Endurance.ReachedMin())
            {
                BaseCharacterAction action = GetCurrentState() as BaseCharacterAction;
                action.Interrupt();
                Sleep sleepAction = NavigateToState("Sleep") as Sleep;
                sleepAction.IncreaseDuration((int)(Endurance.Max / 5f));
                sleepAction.AddOnExit(() =>
                {
                    NavigateToState(action.Name());
                    action.Resume();
                });
                sleepAction.Start();
            }
        }

        public void Rest(float amount)
        {
            Endurance.Val += amount;
            if (Endurance.ReachedMax())
            {
                if (CurrentRegion == null)
                {
                    NavigateToState("Idle");
                }
            }
        }

        public void Travel()
        {
            Tire(CalculateEnduranceCostForDistance(1));
        }

        public float CalculateEnduranceCostForDistance(float distance)
        {
            return distance * CharacterInventory.GetInventoryWeight();
        }

        public void SetWeapon(Weapon weapon)
        {
            _weapon = weapon;
            _weaponName.Text = _weapon.Name();
            _weaponDamage.Val = _weapon.Damage;
            _weaponAccuracy.Val = _weapon.Accuracy;
            _weaponCapacity.Val = _weapon.Capacity;
            _weaponCriticalChance.Val = _weapon.CriticalChance;
            _weaponFireRate.Val = _weapon.FireRate;
            _weaponHandling.Val = _weapon.Handling;
            _weaponReloadSpeed.Val = _weapon.ReloadSpeed;
        }

        public void SetActionListActive(bool active)
        {
            CharacterUi.ActionScrollContent.SetActive(active);
            CharacterUi.CurrentActionText.gameObject.SetActive(!active);
        }

        public Weapon GetWeapon()
        {
            return _weapon;
        }

        public void Initialise(string name, Traits.Trait classCharacter, Traits.Trait characterTrait,
            WeightCategory weight)
        {
            Name = name;
            CharacterClass = classCharacter;
            CharacterTrait = characterTrait;
            Weight = weight;
            actionButtonPrefab = Resources.Load("Prefabs/Action Button") as GameObject;
            SetCharacterUi(gameObject);
            CharacterInventory.MaxWeight = 50;
            AddState(new CollectResources(this));
            AddState(new Combat(this));
            AddState(new Sleep(this));
            AddState(new Idle(this));
            AddState(new PrepareTravel(this));
            AddState(new Travel(this));
            AddState(new Return(this));
            SetDefaultState("Idle");
            UpdateActionUi();
            Strength.AddOnValueChange(delegate(float f)
            {
                CharacterUi.StrengthText.text = f + "+";
                CharacterUi.StrengthTextDetail.text = f + "/" + (int)Strength.Max + " str";
            });
            Endurance.AddOnValueChange(delegate(float f)
            {
                CharacterUi.EnduranceText.text = f + "...";
                CharacterUi.EnduranceTextDetail.text = f + "/" + (int)Endurance.Max + " end";
            });
            Stability.AddOnValueChange(delegate(float f)
            {
                CharacterUi.StabilityText.text = f + "~";
                CharacterUi.StabilityTextDetail.text = f + "/" + (int)Stability.Max + " stb";
            });
            Intelligence.AddOnValueChange(delegate(float f)
            {
                CharacterUi.IntelligenceText.text = f + "?";
                CharacterUi.IntelligenceTextDetail.text = f + "/" + (int)Intelligence.Max + " int";
            });
            Hunger.AddOnValueChange(f => GetHungerStatus(f));
            Thirst.AddOnValueChange(f => GetThirstStatus(f));
        }

        public List<State> StatesAsList(bool includeInactiveStates)
        {
            List<State> states = new List<State>();
            foreach (BaseCharacterAction s in base.StatesAsList())
            {
                if (s.IsStateVisible() || includeInactiveStates)
                {
                    states.Add(s);
                }
            }
            return states;
        }

        private void UpdateActionUi()
        {
            List<BaseCharacterAction> _availableActions = StatesAsList(false).Cast<BaseCharacterAction>().ToList();
            for (int i = 0; i < _availableActions.Count; ++i)
            {
                BaseCharacterAction a = _availableActions[i];
                GameObject newActionButton = Instantiate(actionButtonPrefab);
                a.ActionButtonGameObject = newActionButton;
                newActionButton.transform.SetParent(CharacterUi.ActionScrollContent.transform);
                newActionButton.transform.Find("Text").GetComponent<Text>().text = a.Name();
                Button currentButton = newActionButton.GetComponent<Button>();
                currentButton.GetComponent<Button>().onClick.AddListener(() =>
                {
                    CharacterUi.CollapseCharacterButton.Select();
                    NavigateToState(a.Name());
                });

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

            WorldTime.Instance().MinuteEvent += delegate
            {
                string currentActionString = GetCurrentState().Name() + " " + ((BaseCharacterAction) GetCurrentState()).GetCostAsString();
                CharacterUi.CurrentActionText.text = currentActionString;
                CharacterUi.DetailedCurrentActionText.text = currentActionString;
            };

            _weaponName = new MyString("");
            _weaponName.AddOnValueChange(t =>
            {
                CharacterUi.WeaponNameTextDetailed.text = t;
                CharacterUi.WeaponNameTextSimple.text = t;
            });
            
            _weaponDamage.AddOnValueChange(f => CharacterUi.WeaponDamageText.text = Helper.Round(f, 2) + " dmg");
            _weaponAccuracy.AddOnValueChange(f => CharacterUi.WeaponAccuracyText.text = Helper.Round(f, 2) + "acc");
            _weaponFireRate.AddOnValueChange(f => CharacterUi.WeaponFireRateText.text = Helper.Round(f, 2) + "frt");
            _weaponReloadSpeed.AddOnValueChange(f => CharacterUi.WeaponReloadSpeedText.text = Helper.Round(f, 2) + "rld");
            _weaponCapacity.AddOnValueChange(f => CharacterUi.WeaponCapacityText.text = Helper.Round(f, 2) + "cap");
            _weaponCriticalChance.AddOnValueChange(f => CharacterUi.WeaponCriticalChanceText.text = Helper.Round(f, 2) + "crt");
            _weaponHandling.AddOnValueChange(f => CharacterUi.WeaponHandlingText.text = Helper.Round(f, 2) + "hnd");
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
            float consumed = WorldState.Inventory().DecrementResource("Water", 1);
            Dehydration.Val -= consumed;
        }

        public void Eat()
        {
            float consumed = WorldState.Inventory().DecrementResource("Food", 1);
            Starvation.Val -= consumed;
        }

        public float RemainingCarryCapacity()
        {
            return Strength.Val;
        }
    }
}