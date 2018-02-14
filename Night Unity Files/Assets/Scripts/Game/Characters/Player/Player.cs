using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Assets;
using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Combat;
using Game.Combat.Enemies;
using Game.Combat.Skills;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.World;
using Game.World.Region;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Input;
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
        private Sleep _sleepAction;
        public Idle IdleAction;
        public Travel TravelAction;
        public Return ReturnAction;
        private CraftAmmo _craftAmmoAction;
        private CharacterActions.Combat _combatAction;
        private LightFire _lightFireAction;

        private int _storyProgress;
        public Skill CharacterSkillOne, CharacterSkillTwo;
      

        public string GetCurrentStoryProgress()
        {
            if (_storyProgress == CharacterTemplate.StoryLines.Count) return null;
            string currentLine = CharacterTemplate.StoryLines[_storyProgress];
            ++_storyProgress;
            return currentLine;
        }

        public override void Kill()
        {
            if (SceneManager.GetActiveScene().name == "Game") WorldState.HomeInventory().RemoveItem(this);
            if (CombatManager.Player.Player == this) CombatManager.FailCombat();
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
            
            Energy.OnMin(Sleep);
        }

        ~Player()
        {
            Debug.Log("Destroyed " + Name);
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
            return (from BaseCharacterAction s in States.StatesAsList() where s.IsStateVisible() || includeInactiveStates select s).ToList();
        }

        private void AddStates()
        {
            _collectResourcesAction = new CollectResources(this);
            _combatAction = new CharacterActions.Combat(this);
            _sleepAction = new Sleep(this);
            IdleAction = new Idle(this);
            TravelAction = new Travel(this);
            ReturnAction = new Return(this);
            _lightFireAction = new LightFire(this);
            _craftAmmoAction = new CraftAmmo(this);
            States.SetDefaultState(IdleAction);
            CharacterView.FillActionList();
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

        private void Sleep()
        {
            BaseCharacterAction action = (BaseCharacterAction) States.GetCurrentState();
            action.Interrupt();
            Sleep sleepAction = States.GetState("Sleep") as Sleep;
            if (sleepAction != null)
            {
                sleepAction.SetStateTransitionTarget(action);
                sleepAction.AddOnExit(() => { action.Resume(); });
            }

            _sleepAction.Enter();
        }

        public void Rest(int amount)
        {
            Energy.Increment(amount);
            if (!Energy.ReachedMax()) return;
            if (DistanceFromHome == 0)
            {
                IdleAction.Enter();
            }
        }

        public void Travel()
        {
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
            CharacterView?.WeaponGearUi.SetGearItem(weapon);
        }

        public override void EquipArmour(Armour armour)
        {
            base.EquipArmour(armour);
            CharacterView?.ArmourGearUi.SetGearItem(armour);
        }

        public override void EquipAccessory(Accessory accessory)
        {
            base.EquipAccessory(accessory);
            CharacterView?.AccessoryGearUi.SetGearItem(accessory);
        }

        
        public void CollectResourcesInRegion(Region region)
        {
            _collectResourcesAction.SetTargetRegion(region);
        }
    }
}