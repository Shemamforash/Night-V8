﻿using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Game.Characters.CharacterActions;
using Game.Combat.Player;
using Game.Exploration.WorldEvents;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Persistence;

namespace Game.Characters
{
    public sealed class Player : Character
    {
        public readonly CharacterAttributes Attributes;
        public readonly Skill CharacterSkillOne, CharacterSkillTwo;
        public readonly CharacterTemplate CharacterTemplate;
        public readonly StateMachine States = new StateMachine();
        public readonly BrandManager BrandManager = new BrandManager();
        private readonly List<Effect> _effects = new List<Effect>();

        public Craft CraftAction;
        public LightFire LightFireAction;
        public Consume ConsumeAction;

        private int _storyProgress;
        public CharacterView CharacterView;
        public Rest RestAction;
        public Travel TravelAction;
        public Meditate MeditateAction;
        public Sleep SleepAction;
        private int _timeAlive;
        private bool _storyUnlocked;
        private string _currentStoryLine;

        //Create Character in code only- no view section, no references to objects in the scene
        public Player(CharacterTemplate characterTemplate) : base("The " + characterTemplate.CharacterClass)
        {
            Attributes = new CharacterAttributes(this);
            CharacterTemplate = characterTemplate;
            CharacterSkillOne = CharacterSkills.GetCharacterSkillOne(this);
            CharacterSkillTwo = CharacterSkills.GetCharacterSkillTwo(this);

            AddStates();
            Attributes.Get(AttributeType.Endurance).OnMin(RestAction.Enter);
            BrandManager.Initialise(this);
            UnlockStoryLine();
        }

        public void AddEffect(Effect effect)
        {
            WorldEventManager.GenerateEvent(new CharacterMessage("I feel... different", this));
            _effects.Add(effect);
        }

        public void RemoveEffect(Effect effect)
        {
            WorldEventManager.GenerateEvent(new CharacterMessage("Back to normal", this));
            _effects.Remove(effect);
        }

        public string GetStoryLine()
        {
            string storyLine = _currentStoryLine;
            _currentStoryLine = null;
            return storyLine;
        }

        public bool HasAvailableStoryLine()
        {
            return _currentStoryLine != null;
        }

        private void UnlockStoryLine()
        {
            if (_storyProgress == CharacterTemplate.StoryLines.Count) return;
            _currentStoryLine = CharacterTemplate.StoryLines[_storyProgress];
            ++_storyProgress;
        }

        public void Kill()
        {
            IsDead = true;
        }

        public bool IsDead;

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
            MeditateAction = new Meditate(this);
            SleepAction = new Sleep(this);
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

            UpdateTimeAlive();
        }

        private void UpdateTimeAlive()
        {
            ++_timeAlive;
            if (_timeAlive != WorldState.MinutesPerHour * 24) return;
            UnlockStoryLine();
            BrandManager.IncreaseTimeSurvived();
            _timeAlive = 0;
        }

        public void Tire()
        {
            Attributes.Get(AttributeType.Endurance).Decrement();
            if (Attributes.Val(AttributeType.Endurance) <= 1)
            {
                WorldEventManager.GenerateEvent(new CharacterMessage("I really need some rest", this));
            }
        }

        public void Rest()
        {
            CharacterAttribute endurance = Attributes.Get(AttributeType.Endurance);
            CharacterAttribute perception = Attributes.Get(AttributeType.Perception);
            bool alreadyMaxEndurance = endurance.ReachedMax();
            bool alreadyMaxPerception = perception.ReachedMax();
            endurance.Increment();
            perception.Increment();
            if (!alreadyMaxEndurance && endurance.ReachedMax())
            {
                WorldEventManager.GenerateEvent(new CharacterMessage("I ache, but I am ready", this));
            }

            if (!alreadyMaxPerception && perception.ReachedMax())
            {
                WorldEventManager.GenerateEvent(new CharacterMessage("My mind is sharp, and my eyes keen", this));
            }
        }

        public void Meditate()
        {
            Attributes.Get(AttributeType.Willpower).Increment();
            if (!Attributes.Get(AttributeType.Willpower).ReachedMax()) return;
            WorldEventManager.GenerateEvent(new CharacterMessage("My mind is clear, I can focus now", this));
            RestAction.Enter();
        }

        public void Sleep()
        {
            Attributes.Get(AttributeType.Strength).Increment();
            if (!Attributes.Get(AttributeType.Strength).ReachedMax()) return;
            WorldEventManager.GenerateEvent(new CharacterMessage("My mind is clear, I can focus now", this));
            RestAction.Enter();
        }

        public override void EquipWeapon(Weapon weapon)
        {
            base.EquipWeapon(weapon);
            if (CharacterView != null) CharacterView.WeaponController.SetWeapon(weapon);
            WorldEventManager.GenerateEvent(new CharacterMessage("Yes, this'll do", this));
        }

        public void EquipArmourSlotOne(ArmourPlate plate)
        {
            ArmourController.SetPlateOne(plate);
            CharacterView.ArmourController.SetArmour(ArmourController);
            WorldEventManager.GenerateEvent(new CharacterMessage("That might help", this));
        }

        public void EquipArmourSlotTwo(ArmourPlate plate)
        {
            ArmourController.SetPlateTwo(plate);
            CharacterView.ArmourController.SetArmour(ArmourController);
            WorldEventManager.GenerateEvent(new CharacterMessage("That might help", this));
        }

        public override void EquipAccessory(Accessory accessory)
        {
            base.EquipAccessory(accessory);
            if (CharacterView != null) CharacterView.AccessoryController.SetAccessory(accessory);
        }
    }
}