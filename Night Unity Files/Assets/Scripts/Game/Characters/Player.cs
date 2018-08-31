using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Exploration.WorldEvents;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;

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
        public CharacterView CharacterView;

        public Craft CraftAction;
        public LightFire LightFireAction;
        public Consume ConsumeAction;
        public Rest RestAction;
        public Travel TravelAction;
        public Meditate MeditateAction;
        public Sleep SleepAction;

        private int _storyProgress;
        private int _timeAlive;
        private bool _storyUnlocked;
        private string _currentStoryLine;

        private const int WillPowerGainTarget = 25;
        private int _totalKills;
        private bool _countKills = false;
        
        public bool CanPerformAction()
        {
            return Attributes.Val(AttributeType.Willpower) > 0;
        }
        
        public override XmlNode Save(XmlNode doc)
        {
            doc = base.Save(doc);
            Attributes.Save(doc);
            BrandManager.Save(doc);
            doc.CreateChild("StoryUnlocked", _storyUnlocked);
            doc.CreateChild("StoryProgress", _storyProgress);
            doc.CreateChild("TimeAlive", _timeAlive);
            doc.CreateChild("CharacterClass", CharacterTemplate.CharacterClass.ToString());
            doc.CreateChild("TotalKills", _totalKills);
            doc.CreateChild("CountKills", _countKills);
            ((BaseCharacterAction)States.GetCurrentState()).Save(doc);
            _effects.ForEach(e => e.Save(doc));
            return doc;
        }

        public override void Load(XmlNode root)
        {
            base.Load(root);
            Attributes.Load(root);
            BrandManager.Load(root);
            _storyUnlocked = root.BoolFromNode("StoryUnlocked");
            _storyProgress = root.IntFromNode("StoryProgress");
            _timeAlive = root.IntFromNode("TimeAlive");
            _totalKills = root.IntFromNode("TotalKills");
            _countKills = root.BoolFromNode("CountKills");
            //todo load state
            foreach (XmlNode effectNode in root.SelectNodes("Effect"))
            {
                Effect.Load(this, effectNode);
            }
        }
        
        //Create Character in code only- no view section, no references to objects in the scene
        public Player(CharacterTemplate characterTemplate) : base("The " + characterTemplate.CharacterClass)
        {
            Attributes = new CharacterAttributes(this);
            CharacterTemplate = characterTemplate;
            CharacterSkillOne = CharacterSkills.GetCharacterSkillOne(this);
            CharacterSkillTwo = CharacterSkills.GetCharacterSkillTwo(this);

            AddStates();
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
            if (CharacterTemplate.CharacterClass == CharacterClass.Wanderer)
            {
                SceneChanger.ChangeScene("Game Over");
            }
            else
            {
                CharacterManager.RemoveCharacter(this);
            }
        }

        public bool IsDead;

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
            IncreaseTimeSurvived();
            _timeAlive = 0;
        }

        private void IncreaseTimeSurvived()
        {
            ++_timeSurvived;
            switch (_timeSurvived)
            {
                case 7:
                    Attributes.UnlockCharacterSkillOne();
                    break;
                case 14:
                    Attributes.UnlockCharacterSkillTwo();
                    break;
            }
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
            Rest();
            Attributes.Get(AttributeType.Willpower).Increment();
            if (!Attributes.Get(AttributeType.Willpower).ReachedMax()) return;
            WorldEventManager.GenerateEvent(new CharacterMessage("My mind is clear, I can focus now", this));
            RestAction.Enter();
        }

        public void Sleep()
        {
            Rest();
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
            if (!CombatManager.InCombat()) return;
            PlayerCombat.Instance.EquipWeapon(weapon);
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

        private readonly Dictionary<WeaponType, int> _weaponKills = new Dictionary<WeaponType, int>();
        private int _timeSurvived;

        public void IncreaseKills()
        {
            IncreaseTotalKills();
            IncreaseWeaponKills();
        }

        private void IncreaseWeaponKills()
        {
            WeaponType weaponType = EquippedWeapon.WeaponType();
            if (!_weaponKills.ContainsKey(weaponType))
            {
                _weaponKills.Add(weaponType, 0);
            }

            int kills = _weaponKills[weaponType] + 1;
            _weaponKills[weaponType] = kills;
            switch (kills)
            {
                case 50:
                    Attributes.UnlockWeaponSkillOne(weaponType);
                    break;
                case 100:
                    Attributes.UnlockWeaponSkillTwo(weaponType);
                    break;
            }
        }

        public void StartCountingKills()
        {
            _countKills = true;
        }
        
        private void IncreaseTotalKills()
        {

            if (!_countKills) return;
            ++_totalKills;
            if (_totalKills < WillPowerGainTarget) return;
            _totalKills = 0;
            Attributes.Get(AttributeType.Willpower).Increment();
        }

        public bool CanMeditate()
        {
            return Attributes.Val(AttributeType.Perception) < Attributes.Max(AttributeType.Perception) ||
                   Attributes.Val(AttributeType.Willpower) < Attributes.Max(AttributeType.Willpower);
        }

        public bool CanSleep()
        {
            return Attributes.Val(AttributeType.Strength) < Attributes.Max(AttributeType.Strength) ||
                   Attributes.Val(AttributeType.Endurance) < Attributes.Max(AttributeType.Endurance);
        }
    }
}