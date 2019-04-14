using System;
using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Facilitating.UIControllers;
using Game.Characters.CharacterActions;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
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

        public bool IsDead;
        public Craft CraftAction;
        public Consume ConsumeAction;
        public Rest RestAction;
        public Travel TravelAction;
        public Meditate MeditateAction;

        private readonly Dictionary<WeaponType, int> _weaponKills = new Dictionary<WeaponType, int>();
        private int _daysSurvived;
        private int _timeAlive;
        private bool _showJournal;

        public override XmlNode Save(XmlNode doc)
        {
            doc = base.Save(doc);
            Attributes.Save(doc);
            BrandManager.Save(doc);
            doc.CreateChild("TimeAlive", _timeAlive);
            doc.CreateChild("DaysSurvived", _daysSurvived);
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
            _timeAlive = root.IntFromNode("TimeAlive");
            _daysSurvived = root.IntFromNode("DaysSurvived");

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
            BaseCharacterAction currentAction;
            switch (currentActionName)
            {
                case "Sleep":
                    //todo remove me i am a compatibility option
                    currentAction = RestAction;
                    break;
                case "Meditate":
                    currentAction = MeditateAction;
                    break;
                case "Craft":
                    currentAction = CraftAction;
                    break;
                case "Travel":
                    currentAction = TravelAction;
                    break;
                default:
                    currentAction = RestAction;
                    break;
            }

            if (currentAction != RestAction) currentAction.Load(root);
            currentAction.Enter();
        }

        public bool CanShowJournal()
        {
            if (!_showJournal) return false;
            _showJournal = false;
            return true;
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
            WeaponGenerator.GetWeaponTypes().ForEach(t => { _weaponKills.Add(t, 0); });
        }

        public void Kill(DeathReason deathReason)
        {
            IsDead = true;
            if (CharacterTemplate.CharacterClass == CharacterClass.Wanderer)
                SceneChanger.GoToGameOverScene(deathReason);
            else
                CharacterManager.RemoveCharacter(this);
        }

        private void AddStates()
        {
            RestAction = new Rest(this);
            TravelAction = new Travel(this);
            MeditateAction = new Meditate(this);
            CraftAction = new Craft(this);
            ConsumeAction = new Consume(this);
            States.SetDefaultState(RestAction);
        }

        public void Update()
        {
            if (MapMenuController.CharacterReturning != null) return;
            ((BaseCharacterAction) States.GetCurrentState()).UpdateAction();
            UpdateTimeAlive();
        }

        private void UpdateTimeAlive()
        {
            ++_timeAlive;
            if (_timeAlive != WorldState.MinutesPerHour * 24) return;
            IncreaseTimeSurvived();
            _timeAlive = 0;
        }

        private void IncreaseTimeSurvived()
        {
            if (CharacterManager.CurrentRegion().GetRegionType() == RegionType.Tutorial) return;
            ++_daysSurvived;
            _showJournal = true;
            TryUnlockCharacterSkill(true);
        }

        private const int CharacterSkillOneTarget = 2, CharacterSkillTwoTarget = 4, WeaponSkillOneTarget = 50, WeaponSkillTwoTarget = 150;

        private void TryUnlockCharacterSkill(bool showScreen)
        {
            if (_daysSurvived >= CharacterSkillOneTarget)
                Attributes.UnlockCharacterSkillOne(showScreen);
            if (_daysSurvived >= CharacterSkillTwoTarget)
                Attributes.UnlockCharacterSkillTwo(showScreen);
        }

        public Tuple<string, float> GetCharacterSkillOneProgress() => GetCharacterSkillProgress(CharacterSkillOneTarget);

        public Tuple<string, float> GetCharacterSkillTwoProgress() => GetCharacterSkillProgress(CharacterSkillTwoTarget);

        private Tuple<string, float> GetCharacterSkillProgress(int target)
        {
            int progress = target - _daysSurvived;
            string progressString = "Survive " + progress + " day".Pluralise(progress);
            float normalisedProgress = (float) _daysSurvived / target;
            return Tuple.Create(progressString, normalisedProgress);
        }

        public Tuple<string, float> GetWeaponSkillOneProgress() => GetWeaponProgress(WeaponSkillOneTarget);

        public Tuple<string, float> GetWeaponSkillTwoProgress() => GetWeaponProgress(WeaponSkillTwoTarget);

        private Tuple<string, float> GetWeaponProgress(int target)
        {
            WeaponType weaponType = EquippedWeapon.WeaponType();
            int progress = target - _weaponKills[weaponType];
            string pluralisedEnemy = progress <= 1 ? " enemy" : " enemies";
            string progressString = "Kill " + progress + pluralisedEnemy;
            float normalisedProgress = (float) _weaponKills[weaponType] / target;
            return Tuple.Create(progressString, normalisedProgress);
        }

        public void SetCharacterView(CharacterView characterView)
        {
            _characterView = characterView;
        }

        private readonly string[] _tireEvents =
        {
            "I'm so tired, I need to rest",
            "If only I could close my eyes for a moment",
            "I can't remember when I last had some sleep"
        };

        public void Tire()
        {
            int gritBefore = (int) Attributes.Val(AttributeType.Grit);
            Attributes.Get(AttributeType.Grit).Decrement();
            int gritAfter = (int) Attributes.Val(AttributeType.Grit);
            if (gritBefore == 0 || gritAfter != 0) return;
            WorldEventManager.GenerateEvent(new CharacterMessage(_tireEvents.RandomElement(), this));
        }

        public bool CanAffordTravel(int gritCost = 1)
        {
            int gritRemaining = Mathf.CeilToInt(Attributes.Val(AttributeType.Grit));
            return gritRemaining >= gritCost;
        }

        public void Rest()
        {
            if (!CanRest()) return;
            CharacterAttribute life = Attributes.Get(AttributeType.Life);
            CharacterAttribute grit = Attributes.Get(AttributeType.Grit);
            CharacterAttribute focus = Attributes.Get(AttributeType.Focus);
            CharacterAttribute will = Attributes.Get(AttributeType.Will);
            focus.Increment();
            will.Increment();
            life.Increment();
            grit.Increment();
            if (CanRest()) return;
            WorldEventManager.GenerateEvent(new CharacterMessage(_restEvents.RandomElement(), this));
        }

        private readonly string[] _restEvents =
        {
            "Awake again, or am I still dreaming?",
            "Rested, yet something is not right",
            "Dark day follows dark night, when will it end?"
        };

        public override void EquipWeapon(Weapon weapon)
        {
            base.EquipWeapon(weapon);
            if (_characterView != null) _characterView.WeaponController.UpdateWeapon();
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.EquipWeapon();
        }

        public override void EquipAccessory(Accessory accessory)
        {
            base.EquipAccessory(accessory);
            if (_characterView != null) _characterView.AccessoryController.UpdateAccessory();
        }

        public void IncreaseKills()
        {
            if (CharacterManager.CurrentRegion().GetRegionType() == RegionType.Tutorial) return;
            BrandManager.IncreaseEnemiesKilled();
            WeaponType weaponType = EquippedWeapon.WeaponType();
            _weaponKills[weaponType] = _weaponKills[weaponType] + 1;
            TryUnlockWeaponSkills(weaponType, true);
        }

        private void TryUnlockWeaponSkills(WeaponType weaponType, bool showScreen)
        {
            if (_weaponKills[weaponType] >= WeaponSkillOneTarget)
                Attributes.UnlockWeaponSkillOne(weaponType, showScreen);
            if (_weaponKills[weaponType] >= WeaponSkillTwoTarget)
                Attributes.UnlockWeaponSkillTwo(weaponType, showScreen);
        }

        private bool CanRest()
        {
            return Attributes.Val(AttributeType.Life) < Attributes.Max(AttributeType.Life) ||
                   Attributes.Val(AttributeType.Grit) < Attributes.Max(AttributeType.Grit) ||
                   Attributes.Val(AttributeType.Focus) < Attributes.Max(AttributeType.Focus) ||
                   Attributes.Val(AttributeType.Will) < Attributes.Max(AttributeType.Will);
        }

        public CharacterView CharacterView()
        {
            return _characterView;
        }

        public void ApplyModifier(AttributeType target, AttributeModifier modifier)
        {
            if (!CharacterAttribute.IsCharacterAttribute(target)) return;
            CharacterAttribute attribute = Attributes.Get(target);
            attribute.Max += modifier.RawBonus();
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.RecalculateAttributes();
        }

        public void RemoveModifier(AttributeType target, AttributeModifier modifier)
        {
            if (!CharacterAttribute.IsCharacterAttribute(target)) return;
            CharacterAttribute attribute = Attributes.Get(target);
            attribute.Max -= modifier.RawBonus();
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.RecalculateAttributes();
        }
    }
}