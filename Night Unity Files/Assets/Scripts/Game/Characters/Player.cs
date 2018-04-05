using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Combat.Skills;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.World;
using Game.World.Region;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Characters.Player
{
    public class Player : Character
    {
        public readonly StateMachine States = new StateMachine();
        public readonly CharacterTemplate CharacterTemplate;
        public int DistanceFromHome;
        public CharacterView CharacterView;
        public readonly DesolationAttributes Attributes;
        public readonly Number Energy = new Number();

        private CollectResources _collectResourcesAction;
        public Rest RestAction;
        public Travel TravelAction;
        public Return ReturnAction;
        private CraftAmmo _craftAmmoAction;
        private CharacterActions.Combat _combatAction;
        private LightFire _lightFireAction;
        private BrandManager _brandManager;

        private int _storyProgress;
        public readonly Skill CharacterSkillOne, CharacterSkillTwo;
        public const int PlayerHealthChunkSize = 50;

        public string GetCurrentStoryProgress()
        {
            if (_storyProgress == CharacterTemplate.StoryLines.Count) return null;
            string currentLine = CharacterTemplate.StoryLines[_storyProgress];
            ++_storyProgress;
            return currentLine;
        }

        public override void Kill()
        {
            if (SceneManager.GetActiveScene().name != "Game") return;
            WorldState.HomeInventory().RemoveItem(this);
            WorldState.UnregisterMinuteEvent(UpdateCurrentState);
        }

        //Create Character in code only- no view section, no references to objects in the scene
        public Player(CharacterTemplate characterTemplate) : base("The " + characterTemplate.CharacterClass)
        {
            Debug.Log("Created");
            Attributes = new DesolationAttributes(this);
            CharacterTemplate = characterTemplate;
            CharacterSkillOne = CharacterSkills.GetCharacterSkillOne(this);
            CharacterSkillTwo = CharacterSkills.GetCharacterSkillTwo(this);
            CharacterInventory.MaxWeight = 50;
            Attributes.Endurance.AddOnValueChange(a => { Energy.Max = a.CurrentValue(); });

            _brandManager = new BrandManager(this);
            Energy.OnMin(() => RestAction?.Enter());
        }

        ~Player()
        {
            Debug.Log("Destroyed " + Name);
        }

        public float CalculateDashCooldown()
        {
            return 2f - 0.1f * Attributes.Endurance.CurrentValue();
        }

        private const float MinSpeed = 3, MaxSpeed = 6;

        public float CalculateSpeed()
        {
            float normalisedSpeed = Attributes.Endurance.Normalised();
            return normalisedSpeed * (MaxSpeed - MinSpeed) + MinSpeed;
        }

        public float CalculateDamageModifier()
        {
            return (float) Math.Pow(1.05f, Attributes.Perception.CurrentValue());
        }

        public float CalculateSkillCooldownModifier()
        {
            return (float) Math.Pow(0.95f, Attributes.Willpower.CurrentValue());
        }

        public int CalculateCombatHealth()
        {
            return (int) (Attributes.Strength.CurrentValue() * PlayerHealthChunkSize);
        }

        public override XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            base.Save(doc, saveType);
            SaveController.CreateNodeAndAppend("Class", doc, Name);
            SaveController.CreateNodeAndAppend("Distance", doc, DistanceFromHome);
            SaveController.CreateNodeAndAppend("Energy", doc, Energy.CurrentValue());
            Attributes.Save(doc, saveType);
            return doc;
        }

        public override ViewParent CreateUi(Transform parent)
        {
            return new InventoryUi(this, parent);
        }

        //Links character to object in scene
        public override void SetGameObject(GameObject gameObject)
        {
            base.SetGameObject(gameObject);
            SetCharacterUi();
            AddStates();
        }

        private void SetCharacterUi()
        {
            CharacterView = new CharacterView(this);
        }

        public List<BaseCharacterAction> StatesAsList(bool includeInactiveStates)
        {
            return (from BaseCharacterAction s in States.StatesAsList() where s.IsVisible || includeInactiveStates select s).ToList();
        }

        private void AddStates()
        {
            _collectResourcesAction = new CollectResources(this);
            _combatAction = new CharacterActions.Combat(this);
            RestAction = new Rest(this);
            TravelAction = new Travel(this);
            ReturnAction = new Return(this);
            _lightFireAction = new LightFire(this);
            _craftAmmoAction = new CraftAmmo(this);
            States.SetDefaultState(RestAction);
            CharacterView.FillActionList();
            WorldState.RegisterMinuteEvent(UpdateCurrentState);
        }

        private void UpdateCurrentState()
        {
            ((BaseCharacterAction) States.GetCurrentState()).UpdateAction();
        }

        private bool IsOverburdened()
        {
            return CharacterInventory.Weight >= CharacterInventory.MaxWeight;
        }

        private void Tire()
        {
            int amount = 1;
            if (IsOverburdened()) amount *= 2;
            Energy.Decrement(amount);
        }

        public void Rest(int amount)
        {
            if (Energy.ReachedMax())
            {
                _brandManager.IncreaseIdleTime();
            }

            Energy.Increment(amount);
        }

        private const int HighWeightThreshold = 15, LowWeightThreshold = 5;

        public void Travel()
        {
            if (Inventory().Weight >= HighWeightThreshold)
            {
                _brandManager.IncreaseTimeSpentHighCapacity();
            }
            else if (Inventory().Weight < LowWeightThreshold)
            {
                _brandManager.IncreaseTimeSpentLowCapacity();
            }

            _brandManager.IncreaseTravelTime();
            DistanceFromHome++;
            Tire();
        }

        public void Return()
        {
            --DistanceFromHome;
            Tire();
        }

        public override void EquipWeapon(Weapon weapon)
        {
            base.EquipWeapon(weapon);
            CharacterView?.WeaponController.SetWeapon(weapon);
        }

        public void EquipArmourSlotOne(ArmourPlate plate)
        {
            ArmourController.SetPlateOne(plate);
            CharacterView.ArmourController.SetArmour(ArmourController);
        }

        public void EquipArmourSlotTwo(ArmourPlate plate)
        {
            ArmourController.SetPlateTwo(plate);
            CharacterView.ArmourController.SetArmour(ArmourController);
        }

        public override void EquipAccessory(Accessory accessory)
        {
            base.EquipAccessory(accessory);
            CharacterView?.AccessoryController.SetAccessory(accessory);
        }


        public void CollectResourcesInRegion(Region region)
        {
            _collectResourcesAction.SetTargetRegion(region);
        }
    }
}