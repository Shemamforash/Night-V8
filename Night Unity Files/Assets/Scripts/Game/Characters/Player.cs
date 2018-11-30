using System.Collections.Generic;
using System.Xml;
using Facilitating.Persistence;
using Game.Characters.Brands;
using Game.Characters.CharacterActions;
using Game.Combat.Player;
using Game.Exploration.WorldEvents;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.Global;
using NUnit.Framework;
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
        public Sleep SleepAction;

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
                    currentAction = SleepAction;
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

        public void Kill()
        {
            IsDead = true;
            if (CharacterTemplate.CharacterClass == CharacterClass.Wanderer)
                SceneChanger.GoToGameOverScene();
            else
                CharacterManager.RemoveCharacter(this);
        }

        private void AddStates()
        {
            RestAction = new Rest(this);
            TravelAction = new Travel(this);
            MeditateAction = new Meditate(this);
            SleepAction = new Sleep(this);
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
            IncreaseTimeSurvived();
            _timeAlive = 0;
        }

        private void IncreaseTimeSurvived()
        {
            ++_daysSurvived;
            _showJournal = true;
            TryUnlockCharacterSkill(true);
        }

        private void TryUnlockCharacterSkill(bool showScreen)
        {
            if (_daysSurvived >= 7)
                Attributes.UnlockCharacterSkillOne(showScreen);
            if (_daysSurvived >= 14)
                Attributes.UnlockCharacterSkillTwo(showScreen);
        }

        public void SetCharacterView(CharacterView characterView)
        {
            _characterView = characterView;
        }

        public void Tire()
        {
            Attributes.Get(AttributeType.Grit).Decrement();
            if (Attributes.Val(AttributeType.Grit) <= 1)
            {
                WorldEventManager.GenerateEvent(new CharacterMessage("I really need some rest", this));
            }
        }

        public bool CanAffordTravel(int gritCost = 1)
        {
            int gritRemaining = Mathf.CeilToInt(Attributes.Val(AttributeType.Grit));
            return gritRemaining >= gritCost;
        }

        public void Sleep()
        {
            CharacterAttribute fettle = Attributes.Get(AttributeType.Fettle);
            CharacterAttribute grit = Attributes.Get(AttributeType.Grit);
            CharacterAttribute focus = Attributes.Get(AttributeType.Focus);
            CharacterAttribute will = Attributes.Get(AttributeType.Will);
            focus.Increment();
            will.Increment();
            fettle.Increment();
            grit.Increment();
            if (!(fettle.ReachedMax() && grit.ReachedMax() && focus.ReachedMax() && will.ReachedMax())) return;
            WorldEventManager.GenerateEvent(new CharacterMessage("My mind is clear, I can focus now", this));
            RestAction.Enter();
        }

        public override void EquipWeapon(Weapon weapon)
        {
            base.EquipWeapon(weapon);
            if (_characterView != null) _characterView.WeaponController.UpdateWeapon();
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.EquipWeapon(weapon);
        }

        public void EquipChestArmour(Armour plate)
        {
            ArmourController.SetArmour(plate);
            WorldEventManager.GenerateEvent(new CharacterMessage("That might help", this));
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.EquipArmour();
        }

        public void EquipHeadArmour(Armour plate)
        {
            ArmourController.SetArmour(plate);
            if (PlayerCombat.Instance == null) return;
            PlayerCombat.Instance.EquipArmour();
        }

        public override void EquipAccessory(Accessory accessory)
        {
            base.EquipAccessory(accessory);
            if (_characterView != null) _characterView.AccessoryController.UpdateAccessory();
        }

        public void IncreaseKills()
        {
            BrandManager.IncreaseEnemiesKilled();
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

        public bool CanSleep()
        {
            return Attributes.Val(AttributeType.Fettle) < Attributes.Max(AttributeType.Fettle) ||
                   Attributes.Val(AttributeType.Grit) < Attributes.Max(AttributeType.Grit) ||
                   Attributes.Val(AttributeType.Focus) < Attributes.Max(AttributeType.Focus) ||
                   Attributes.Val(AttributeType.Will) < Attributes.Max(AttributeType.Will);
        }

        public CharacterView CharacterView()
        {
            return _characterView;
        }

        public int GetMaxSleepTime()
        {
            int grit = Mathf.CeilToInt(Attributes.Val(AttributeType.Grit));
            int gritMax = Mathf.CeilToInt(Attributes.Max(AttributeType.Grit));
            int gritLoss = gritMax - grit;

            int fettle = Mathf.CeilToInt(Attributes.Val(AttributeType.Fettle));
            int fettleMax = Mathf.CeilToInt(Attributes.Max(AttributeType.Fettle));
            int fettleLoss = fettleMax - fettle;

            int will = Mathf.CeilToInt(Attributes.Val(AttributeType.Will));
            int willMax = Mathf.CeilToInt(Attributes.Max(AttributeType.Will));
            int willLoss = willMax - will;

            int focus = Mathf.CeilToInt(Attributes.Val(AttributeType.Focus));
            int focusMax = Mathf.CeilToInt(Attributes.Max(AttributeType.Focus));
            int focusLoss = focusMax - focus;

            return Mathf.Max(gritLoss, fettleLoss, focusLoss, willLoss);
        }

        public bool IsConsuming()
        {
            State currentState = States.GetCurrentState();
            return currentState != MeditateAction && currentState != SleepAction;
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