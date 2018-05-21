﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game.Characters.CharacterActions;
using Game.Combat.Player;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Persistence;
using UnityEngine.SceneManagement;

namespace Game.Characters
{
    public sealed class Player : Character
    {
        public const int PlayerHealthChunkSize = 50;

        private const float MinSpeed = 3, MaxSpeed = 6;

        private const int HighWeightThreshold = 15, LowWeightThreshold = 5;
        public readonly DesolationAttributes Attributes;
        public readonly Skill CharacterSkillOne, CharacterSkillTwo;
        public readonly CharacterTemplate CharacterTemplate;
        public readonly StateMachine States = new StateMachine();
        private readonly BrandManager _brandManager;

        public CraftAmmo CraftAction;
        private LightFire _lightFireAction;

        private int _storyProgress;
        public CharacterView CharacterView;
        public Rest RestAction;
        public Travel TravelAction;

        //Create Character in code only- no view section, no references to objects in the scene
        public Player(CharacterTemplate characterTemplate) : base("The " + characterTemplate.CharacterClass)
        {
            Attributes = new DesolationAttributes(this);
            CharacterTemplate = characterTemplate;
            CharacterSkillOne = CharacterSkills.GetCharacterSkillOne(this);
            CharacterSkillTwo = CharacterSkills.GetCharacterSkillTwo(this);
            CharacterInventory.MaxWeight = 50;

            _brandManager = new BrandManager(this);
            AddStates();
            Attributes.Endurance.OnMin(RestAction.Enter);
        }

        public bool ConsumeResource(InventoryResourceType type, int amount)
        {
            DesolationInventory inventory = TravelAction.AtHome() ? WorldState.HomeInventory() : Inventory();
            return inventory.DecrementResource(type, amount);
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

        public float CalculateDashCooldown()
        {
            return 2f - 0.1f * Attributes.Endurance.CurrentValue();
        }

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
            _lightFireAction = new LightFire(this);
            CraftAction = new CraftAmmo(this);
            States.SetDefaultState(RestAction);
        }

        public void UpdateCurrentState()
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
            Attributes.Endurance.Decrement(amount);
        }

        public void Rest(int amount)
        {
            if (Attributes.Endurance.ReachedMax()) _brandManager.IncreaseIdleTime();
            Attributes.Endurance.Increment(amount);
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
    }
}