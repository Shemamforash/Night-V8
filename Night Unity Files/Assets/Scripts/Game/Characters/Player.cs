using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game.Characters.CharacterActions;
using Game.Combat.Player;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Characters
{
    public sealed class Player : Character
    {
        private const int HighWeightThreshold = 15, LowWeightThreshold = 5;
        public readonly CharacterAttributes Attributes;
        public readonly Skill CharacterSkillOne, CharacterSkillTwo;
        public readonly CharacterTemplate CharacterTemplate;
        public readonly StateMachine States = new StateMachine();
        private readonly BrandManager _brandManager;
        private readonly List<Effect> _effects = new List<Effect>();

        public Craft CraftAction;
        public LightFire LightFireAction;
        public Consume ConsumeAction;

        private int _storyProgress;
        public CharacterView CharacterView;
        public Rest RestAction;
        public Travel TravelAction;

        //Create Character in code only- no view section, no references to objects in the scene
        public Player(CharacterTemplate characterTemplate) : base("The " + characterTemplate.CharacterClass)
        {
            Attributes = new CharacterAttributes(this);
            CharacterTemplate = characterTemplate;
            CharacterSkillOne = CharacterSkills.GetCharacterSkillOne(this);
            CharacterSkillTwo = CharacterSkills.GetCharacterSkillTwo(this);
            CharacterInventory.MaxWeight = 50;

            _brandManager = new BrandManager(this);
            AddStates();
            Attributes.Get(AttributeType.Endurance).OnMin(RestAction.Enter);
        }

//        public bool ConsumeResource(InventoryResourceType type, int amount)
//        {
//            DesolationInventory inventory = TravelAction.AtHome() ? WorldState.HomeInventory() : Inventory();
//            return inventory.DecrementResource(type, amount);
//        }

        public void AddEffect(Effect effect)
        {
            _effects.Add(effect);
        }

        public void RemoveEffect(Effect effect)
        {
            _effects.Remove(effect);
        }
        
        public string GetCurrentStoryProgress()
        {
            if (_storyProgress == CharacterTemplate.StoryLines.Count) return null;
            string currentLine = CharacterTemplate.StoryLines[_storyProgress];
            ++_storyProgress;
            return currentLine;
        }

        public void Kill()
        {
            if (SceneManager.GetActiveScene().name != "Game") return;
            WorldState.HomeInventory().RemoveCharacter(this);
        }

        ~Player()
        {
//            Debug.Log("Destroyed " + Name);
        }

        public override XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            doc = base.Save(doc, saveType);
            Attributes.Save(doc, saveType);
            return doc;
        }

        public List<BaseCharacterAction> StatesAsList(bool includeInactiveStates)
        {
            return (from BaseCharacterAction s in States.StatesAsList() where s.IsVisible || includeInactiveStates select s).ToList();
        }

        private void AddStates()
        {
            RestAction = new Rest(this);
            TravelAction = new Travel(this);
            LightFireAction = new LightFire(this);
            CraftAction = new Craft(this);
            ConsumeAction = new Consume(this);
            States.SetDefaultState(RestAction);
        }

        public void Update()
        {
            ((BaseCharacterAction) States.GetCurrentState()).UpdateAction();
            for (int i = _effects.Count - 1; i >= 0; --i)
            {
                _effects[i].UpdateEffect();
            }
        }

        private bool IsOverburdened()
        {
            return CharacterInventory.Weight >= CharacterInventory.MaxWeight;
        }

        private void Tire()
        {
            int amount = 1;
            if (IsOverburdened()) amount *= 2;
            Attributes.Get(AttributeType.Endurance).Decrement(amount);
        }

        public void Rest(int amount)
        {
            if (Attributes.Get(AttributeType.Endurance).ReachedMax())
            {
                _brandManager.IncreaseIdleTime();
                return;
            }
            Attributes.Get(AttributeType.Endurance).Increment(amount);
        }

        public void Travel()
        {
            if (Inventory().Weight >= HighWeightThreshold)
                _brandManager.IncreaseTimeSpentHighCapacity();
            else if (Inventory().Weight < LowWeightThreshold)
                _brandManager.IncreaseTimeSpentLowCapacity();

            _brandManager.IncreaseTravelTime();
            Tire();
        }

        public override void EquipWeapon(Weapon weapon)
        {
            base.EquipWeapon(weapon);
            if(CharacterView != null) CharacterView.WeaponController.SetWeapon(weapon);
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
    }
}