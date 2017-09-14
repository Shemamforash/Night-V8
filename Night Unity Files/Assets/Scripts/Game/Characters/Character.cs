using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Characters;
using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Combat.Weapons;
using Game.World;
using Game.World.Region;
using Game.World.Time;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI.CustomTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Characters
{
    public class Character : StateMachine, IPersistenceTemplate
    {
        public CharacterUI CharacterUi;
        public string CharacterName;
        public CharacterAttributes Attributes;

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

        private Weapon _weapon;

        public void AddItemToInventory(InventoryItem item)
        {
            CharacterInventory.AddItem(item);
        }

        public void TakeDamage(int amount)
        {
            Attributes.Strength.Val -= amount;
            if (Attributes.Strength.ReachedMin())
            {
                //TODO kill character
            }
        }

        private bool IsOverburdened()
        {
            return CharacterInventory.GetInventoryWeight() > Attributes.Strength.Val;
        }

        private void Tire(int amount)
        {
            Attributes.Endurance.Val -= IsOverburdened() ? amount * 2 : amount;
            CheckEnduranceZero();
        }

        private void CheckEnduranceZero()
        {
            if (!Attributes.Endurance.ReachedMin()) return;
            BaseCharacterAction action = GetCurrentState() as BaseCharacterAction;
            action.Interrupt();
            Sleep sleepAction = NavigateToState("Sleep") as Sleep;
            sleepAction.SetDuration((int)(Attributes.Endurance.Max / 5f));
            sleepAction.SetStateTransitionTarget(action.Name());
            sleepAction.AddOnExit(() =>
            {
                action.Resume();
            });
            sleepAction.Start();
        }

        public void Rest(int amount)
        {
            Attributes.Endurance.Val += amount;
            if (Attributes.Endurance.ReachedMax())
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

        public int CalculateTotalWeight()
        {
            int characterWeight = 5 + (int) Attributes.Weight;
            int inventoryWeight = (int)(CharacterInventory.GetInventoryWeight() / 10);
            return characterWeight + inventoryWeight;
        }
        
        public int CalculateEnduranceCostForDistance(int distance)
        {
            return distance * CalculateTotalWeight();
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
            CharacterUi.DetailedCurrentActionText.gameObject.SetActive(!active);
        }

        public Weapon GetWeapon()
        {
            return _weapon;
        }

        public void Awake()
        {
            AddState(new CollectResources(this));
            AddState(new CharacterActions.Combat(this));
            AddState(new Sleep(this));
            AddState(new Idle(this));
            AddState(new PrepareTravel(this));
            AddState(new Travel(this));
            AddState(new Return(this));
            Attributes = new CharacterAttributes(this);
        }

        public void Initialise(string characterName, Traits.Trait classCharacter, Traits.Trait characterTrait)
        {
            CharacterName = characterName;
            CharacterClass = classCharacter;
            CharacterTrait = characterTrait;
            SetCharacterUi(gameObject);
            CharacterInventory.MaxWeight = 50;
            UpdateActionUi();
            SetDefaultState("Idle");
        }
        
        private void SetCharacterUi(GameObject g)
        {
            CharacterUi = new CharacterUI(g);
            CharacterUi.EatButton.onClick.AddListener(Eat);
            CharacterUi.DrinkButton.onClick.AddListener(Drink);

            CharacterUi.NameText.text = CharacterName;
            CharacterUi.ClassTraitText.text = CharacterTrait.Name + " " + CharacterClass.Name;
            CharacterUi.DetailedClassText.text = CharacterClass.GetTraitDetails();
            CharacterUi.DetailedTraitText.text = CharacterTrait.GetTraitDetails();
            CharacterUi.WeightText.text = "Weight: " + Attributes.Weight + " (requires " + ((int) Attributes.Weight + 5) + " fuel)";

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
            
            Attributes.Strength.AddOnValueChange(delegate(int f)
            {
                CharacterUi.StrengthText.text = f + " <sprite name=\"Strength\">";
                CharacterUi.StrengthTextDetail.text = f + "/" + Attributes.Strength.Max + " <sprite name=\"Strength\">";
            });
            Attributes.Endurance.AddOnValueChange(delegate(int f)
            {
                CharacterUi.EnduranceText.text = f + " <sprite name=\"Endurance\">";
                CharacterUi.EnduranceTextDetail.text = f + "/" + Attributes.Endurance.Max + " <sprite name=\"Endurance\">";
            });
            Attributes.Stability.AddOnValueChange(delegate(int f)
            {
                CharacterUi.StabilityText.text = f + " <sprite name=\"Stability\">";
                CharacterUi.StabilityTextDetail.text = f + "/" + Attributes.Stability.Max + " <sprite name=\"Stability\">";
            });
            Attributes.Intelligence.AddOnValueChange(delegate(int f)
            {
                CharacterUi.IntelligenceText.text = f + " <sprite name=\"Intelligence\">";
                CharacterUi.IntelligenceTextDetail.text = f + "/" + Attributes.Intelligence.Max + " <sprite name=\"Intelligence\">";
            });
            Attributes.Hunger.AddOnValueChange(f => CharacterUi.HungerText.text = Attributes.GetHungerStatus());
            Attributes.Thirst.AddOnValueChange(f => CharacterUi.ThirstText.text = Attributes.GetThirstStatus());
        }


        private List<State> StatesAsList(bool includeInactiveStates)
        {
            List<State> states = new List<State>();
            foreach (BaseCharacterAction s in StatesAsList())
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
                GameObject newActionButton = Helper.InstantiateUiObject("Prefabs/Action Button", CharacterUi.ActionScrollContent.transform);
                a.ActionButtonGameObject = newActionButton;
                newActionButton.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = a.Name();
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

        public string GetConditions()
        {
            string conditions = "";
            conditions += Attributes.GetThirstStatus() + "(" + Mathf.Round(Attributes.Thirst.Val / 1.2f) / 10f + " litres/hr)\n";
            conditions += Attributes.GetThirstStatus() + "(" + Mathf.Round(Attributes.Hunger.Val / 1.2f) / 10f + " meals/hr)";
            return conditions;
        }

        public void Kill()
        {
            CharacterManager.RemoveCharacter(this, CharacterName == "Driver");
        }

        public CharacterUI GetCharacterUi()
        {
            return CharacterUi;
        }

        public void Drink()
        {
            int consumed = WorldState.Inventory().DecrementResource("Water", 1);
            Attributes.Dehydration.Val -= consumed;
        }

        public void Eat()
        {
            int consumed = WorldState.Inventory().DecrementResource("Food", 1);
            Attributes.Starvation.Val -= consumed;
        }

        public float RemainingCarryCapacity()
        {
            return Attributes.Strength.Val;
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            CharacterName = doc.SelectSingleNode("Name").InnerText;
            XmlNode attributesNode = doc.SelectSingleNode("Attributes");
            Attributes.Load(attributesNode, saveType);
        }

        public void Save(XmlNode doc, PersistenceType saveType)
        {
            SaveController.CreateNodeAndAppend("Name", doc, CharacterName);
            XmlNode attributesNode = SaveController.CreateNodeAndAppend("Attributes", doc);
            Attributes.Save(attributesNode, saveType);
        }
    }
}