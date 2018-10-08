using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Combat.Player;
using Game.Exploration.WorldEvents;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Characters
{
    public sealed class Player : Character
    {
        public readonly CharacterAttributes Attributes;
        public readonly Skill CharacterSkillOne, CharacterSkillTwo;
        public readonly CharacterTemplate CharacterTemplate;
        public readonly StateMachine States = new StateMachine();
        public readonly BrandManager BrandManager = new BrandManager();
        private CharacterView _characterView;

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
            XmlNode weaponKillNode = doc.CreateChild("WeaponKills");
            foreach (KeyValuePair<WeaponType, int> weaponKills in _weaponKills)
                weaponKillNode.CreateChild(weaponKills.Key.ToString(), weaponKills.Value);

            ((BaseCharacterAction) States.GetCurrentState()).Save(doc);
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

            XmlNode weaponKillNode = root.SelectSingleNode("WeaponKills");
            List<WeaponType> weaponTypes = WeaponGenerator.GetWeaponTypes();
            weaponTypes.ForEach(t =>
            {
                _weaponKills[t] = weaponKillNode.IntFromNode(t.ToString());
                TryUnlockWeaponSkills(t, false);
            });

            TryUnlockCharacterSkill(false);
            LoadCurrentAction(root);
        }

        private void LoadCurrentAction(XmlNode root)
        {
            XmlNode currentActionNode = root.SelectSingleNode("CurrentAction");
            string currentActionName = currentActionNode.StringFromNode("Name");
            BaseCharacterAction currentAction = null;
            switch (currentActionName)
            {
                case "Rest":
                    currentAction = RestAction;
                    break;
                case "Sleep":
                    currentAction = SleepAction;
                    break;
                case "Meditate":
                    currentAction = MeditateAction;
                    break;
                case "LightFire":
                    currentAction = LightFireAction;
                    break;
                case "Craft":
                    currentAction = CraftAction;
                    break;
                case "Travel":
                    currentAction = TravelAction;
                    break;
            }

            currentAction.Enter();
            currentAction.Load(root);
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

            WeaponGenerator.GetWeaponTypes().ForEach(t => { _weaponKills.Add(t, 0); });
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
                SceneChanger.GoToGameOverScene();
            else
                CharacterManager.RemoveCharacter(this);
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
            TryUnlockCharacterSkill(true);
        }

        private void TryUnlockCharacterSkill(bool showScreen)
        {
            if (_timeSurvived >= 7)
                Attributes.UnlockCharacterSkillOne(showScreen);
            if (_timeSurvived >= 14)
                Attributes.UnlockCharacterSkillTwo(showScreen);
        }

        public void SetCharacterView(CharacterView characterView)
        {
            _characterView = characterView;
        }

        public void Tire()
        {
            Attributes.Get(AttributeType.Endurance).Decrement();
            if (Attributes.Val(AttributeType.Endurance) <= 1)
            {
                WorldEventManager.GenerateEvent(new CharacterMessage("I really need some rest", this));
            }
        }

        public void Meditate()
        {
            CharacterAttribute perception = Attributes.Get(AttributeType.Perception);
            CharacterAttribute willpower = Attributes.Get(AttributeType.Willpower);
            perception.Increment();
            willpower.Increment();
            if (!(perception.ReachedMax() && willpower.ReachedMax())) return;
            WorldEventManager.GenerateEvent(new CharacterMessage("My mind is clear, I can focus now", this));
            RestAction.Enter();
        }

        public bool CanAffordTravel(int enduranceCost = 1)
        {
            int enduranceRemaining = Mathf.CeilToInt(Attributes.Val(AttributeType.Endurance));
            int willRemaining = Mathf.CeilToInt(Attributes.Val(AttributeType.Willpower));
            return enduranceRemaining >= enduranceCost && willRemaining > 0;
        }

        public void Sleep()
        {
            CharacterAttribute strength = Attributes.Get(AttributeType.Strength);
            CharacterAttribute endurance = Attributes.Get(AttributeType.Endurance);
            strength.Increment();
            endurance.Increment();
            if (!(strength.ReachedMax() && endurance.ReachedMax())) return;
            WorldEventManager.GenerateEvent(new CharacterMessage("My mind is clear, I can focus now", this));
            RestAction.Enter();
        }

        public override void EquipWeapon(Weapon weapon)
        {
            base.EquipWeapon(weapon);
            if (_characterView != null) _characterView.WeaponController.SetWeapon(weapon);
            WorldEventManager.GenerateEvent(new CharacterMessage("Yes, this'll do", this));
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.EquipWeapon(weapon);
        }

        public void EquipChestArmour(Armour plate)
        {
            ArmourController.SetArmour(plate);
            _characterView.ArmourController.SetArmour(ArmourController);
            WorldEventManager.GenerateEvent(new CharacterMessage("That might help", this));
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.EquipArmour();
        }

        public void EquipHeadArmour(Armour plate)
        {
            ArmourController.SetArmour(plate);
            _characterView.ArmourController.SetArmour(ArmourController);
            WorldEventManager.GenerateEvent(new CharacterMessage("That might help", this));
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.EquipArmour();
        }

        public override void EquipAccessory(Accessory accessory)
        {
            base.EquipAccessory(accessory);
            if (_characterView != null) _characterView.AccessoryController.SetAccessory(accessory);
        }

        private readonly Dictionary<WeaponType, int> _weaponKills = new Dictionary<WeaponType, int>();
        private int _timeSurvived;

        public void IncreaseKills()
        {
            WeaponType weaponType = EquippedWeapon.WeaponType();
            _weaponKills[weaponType] = _weaponKills[weaponType] + 1;
            TryUnlockWeaponSkills(weaponType, true);
        }

        private void TryUnlockWeaponSkills(WeaponType weaponType, bool showScreen)
        {
            if (_weaponKills[weaponType] >= 50)
                Attributes.UnlockWeaponSkillOne(weaponType, showScreen);
            if (_weaponKills[weaponType] >= 100)
                Attributes.UnlockWeaponSkillTwo(weaponType, showScreen);
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

        public CharacterView CharacterView()
        {
            return _characterView;
        }

        public int GetMaxMeditateTime()
        {
            int willpower = Mathf.CeilToInt(Attributes.Val(AttributeType.Willpower));
            int willpowerMax = Mathf.CeilToInt(Attributes.Max(AttributeType.Willpower));
            int willpowerLoss = willpowerMax - willpower;

            int perception = Mathf.CeilToInt(Attributes.Val(AttributeType.Perception));
            int perceptionMax = Mathf.CeilToInt(Attributes.Max(AttributeType.Perception));
            int perceptionLoss = perceptionMax - perception;

            return Mathf.Max(willpowerLoss, perceptionLoss);
        }

        public int GetMaxSleepTime()
        {
            int endurance = Mathf.CeilToInt(Attributes.Val(AttributeType.Endurance));
            int enduranceMax = Mathf.CeilToInt(Attributes.Max(AttributeType.Endurance));
            int enduranceLoss = enduranceMax - endurance;

            int strength = Mathf.CeilToInt(Attributes.Val(AttributeType.Strength));
            int strengthMax = Mathf.CeilToInt(Attributes.Max(AttributeType.Strength));
            int strengthLoss = strengthMax - strength;

            return Mathf.Max(enduranceLoss, strengthLoss);
        }

        public bool IsConsuming()
        {
            State currentState = States.GetCurrentState();
            return currentState != MeditateAction && currentState != SleepAction;
        }
    }
}