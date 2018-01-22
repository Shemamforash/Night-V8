using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Assets;
using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Combat;
using Game.Combat.Skills;
using Game.Gear.Weapons;
using Game.World.Region;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Input;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEngine;

namespace Game.Characters.Player
{
    public class Player : Character, IInputListener
    {
        public readonly StateMachine States = new StateMachine();
        public readonly TraitLoader.Trait CharacterTrait;
        public readonly TraitLoader.CharacterClass CharacterClass;
        public int DistanceFromHome;
        public CharacterView CharacterView;
        public readonly DesolationAttributes Attributes;
        public readonly Number Energy = new Number();

        public CollectResources CollectResourcesAction;
        public Sleep SleepAction;
        public Idle IdleAction;
        public Travel TravelAction;
        public Return ReturnAction;
        public CraftAmmo CraftAmmoAction;
        public CharacterActions.Combat CombatAction;
        public LightFire LightFireAction;
        public RageController RageController;

        private int _storyProgress;

        public string GetCurrentStoryProgress()
        {
            if (_storyProgress == CharacterClass.StoryLines.Count) return null;
            string currentLine = CharacterClass.StoryLines[_storyProgress];
            ++_storyProgress;
            return currentLine;
        }

        //Create Character in code only- no view section, no references to objects in the scene
        public Player(string name, TraitLoader.CharacterClass characterClass, TraitLoader.Trait characterTrait) : base(name)
        {
            Attributes = new DesolationAttributes(this);
            MovementController = new MovementController(this, 0);
            CharacterClass = characterClass;
            CharacterTrait = characterTrait;
            CharacterInventory.MaxWeight = 50;
            Attributes.Endurance.AddOnValueChange(a =>
            {
                Energy.Max = a.CurrentValue();
                MovementController.SetBaseSpeed((int) a.CurrentValue());
            });
            RageController = new RageController(this);
            HealthController.AddOnHeal(a => UpdateHealthUi(HealthController.GetNormalisedHealthValue()));
            HealthController.AddOnTakeDamage(a => UpdateHealthUi(HealthController.GetNormalisedHealthValue()));
            Energy.OnMin(Sleep);
            LinkCooldownsToUi();
            TakeCoverAction = () => CombatManager.SetCoverText("In Cover");
            LeaveCoverAction = () => CombatManager.SetCoverText("Exposed");
            SetConditions();
            Position.AddOnValueChange(a => CombatManager.GetEnemies().ForEach(e => e.Position.UpdateValueChange()));
        }

        private void LinkCooldownsToUi()
        {
            LinkCockCooldownToUi();
            LinkKnockdownCooldownToUi();
            LinkReloadCooldownToUi();
        }

        public override void KnockDown()
        {
            if (!KnockedDown)
                base.KnockDown();
            UIKnockdownController.StartKnockdown(10);
        }

        private void UpdateHealthUi(float normalisedHealth)
        {
            HeartBeatController.SetHealth(normalisedHealth);
            CombatManager.UpdatePlayerHealth();
        }

        public override XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            base.Save(doc, saveType);
            SaveController.CreateNodeAndAppend("Class", doc, CharacterClass.Name);
            SaveController.CreateNodeAndAppend("Trait", doc, CharacterTrait.Name);
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
            CollectResourcesAction = new CollectResources(this);
            CombatAction = new CharacterActions.Combat(this);
            SleepAction = new Sleep(this);
            IdleAction = new Idle(this);
            TravelAction = new Travel(this);
            ReturnAction = new Return(this);
            LightFireAction = new LightFire(this);
            CraftAmmoAction = new CraftAmmo(this);
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
            sleepAction.SetStateTransitionTarget(action);
            sleepAction.AddOnExit(() => { action.Resume(); });
            SleepAction.Enter();
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

        public int CalculateTotalWeight()
        {
            int characterWeight = 5 + Attributes.Weight;
            int inventoryWeight = (int) (CharacterInventory.Weight / 10);
            return characterWeight + inventoryWeight;
        }

        public override void Equip(GearItem gearItem)
        {
            base.Equip(gearItem);
            switch (gearItem.GetGearType())
            {
                case GearSubtype.Weapon:
                    SwapWeaponSkills((Weapon) gearItem);
                    ((Weapon) gearItem).Reload(Inventory());
                    CharacterView?.WeaponGearUi.SetGearItem(gearItem);
                    break;
                case GearSubtype.Armour:
                    CharacterView?.ArmourGearUi.SetGearItem(gearItem);
                    break;
                case GearSubtype.Accessory:
                    CharacterView?.AccessoryGearUi.SetGearItem(gearItem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SwapWeaponSkills(Weapon weapon)
        {
            switch (weapon.WeaponType())
            {
                case WeaponType.Pistol:
                    CombatManager.SkillBar.BindSkill(3, new Skill.Retribution(this));
                    CombatManager.SkillBar.BindSkill(4, new Skill.Revenge(this));
                    break;
                case WeaponType.Rifle:
                    CombatManager.SkillBar.BindSkill(3, new Skill.PiercingShot(this));
                    CombatManager.SkillBar.BindSkill(4, new Skill.FullBlast(this));
                    break;
                case WeaponType.Shotgun:
                    CombatManager.SkillBar.BindSkill(3, new Skill.LegSweep(this));
                    CombatManager.SkillBar.BindSkill(4, new Skill.BulletCloud(this));
                    break;
                case WeaponType.SMG:
                    CombatManager.SkillBar.BindSkill(3, new Skill.DoubleUp(this));
                    CombatManager.SkillBar.BindSkill(4, new Skill.Splinter(this));
                    break;
                case WeaponType.LMG:
                    CombatManager.SkillBar.BindSkill(3, new Skill.TopUp(this));
                    CombatManager.SkillBar.BindSkill(4, new Skill.HeavyLead(this));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //COOLDOWNS

        protected void LinkCockCooldownToUi()
        {
            CockingCooldown.SetEndAction(() =>
            {
                EquipmentController.Weapon().Cocked = true;
                UpdateMagazineUi();
            });
            CockingCooldown.SetStartAction(() => UIMagazineController.SetMessage("Cocking"));
        }

        protected void LinkKnockdownCooldownToUi()
        {
            //todo
        }

        protected void LinkReloadCooldownToUi()
        {
            ReloadingCooldown.SetEndAction(() =>
            {
                EquipmentController.Weapon().Cocked = true;
                EquipmentController.Weapon().Reload(Inventory());
                UpdateMagazineUi();
            });
            ReloadingCooldown.SetDuringAction(UIMagazineController.UpdateReloadTime);
        }

        //FIRING
        protected override Shot FireWeapon(Character target)
        {
            Shot shot = base.FireWeapon(target);
            if (RageController.Active()) shot.GuaranteeCritical();
            UpdateMagazineUi();
            return shot;
        }

        //MISC

        protected override void Interrupt()
        {
            base.Interrupt();
            MovementController.StopSprinting();
            UpdateMagazineUi();
        }

        public void UpdateMagazineUi()
        {
            string magazineMessage = "";
            if (Weapon().GetRemainingMagazines() == 0) magazineMessage = "NO AMMO";
            else if (!EquipmentController.Weapon().Cocked)
                magazineMessage = "EJECT CARTRIDGE";
            else if (EquipmentController.Weapon().Empty())
                magazineMessage = "RELOAD";
            if (magazineMessage == "")
            {
                UIMagazineController.UpdateMagazine();
            }
            else
            {
                UIMagazineController.EmptyMagazine();
                UIMagazineController.SetMessage(magazineMessage);
            }
        }

        //INPUT

        public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            if (Immobilised()) return;
            switch (axis)
            {
                case InputAxis.CancelCover:
                    TakeCover();
                    break;
                case InputAxis.Submit:
                    RageController.TryStart();
                    break;
                case InputAxis.Fire:
                    FireWeapon(null);
                    break;
                case InputAxis.Reload:
                    TryReload();
                    break;
                case InputAxis.Horizontal:
                    MovementController.Move(direction);
                    break;
                case InputAxis.Sprint:
                    MovementController.StartSprinting();
                    break;
                case InputAxis.SkillOne:
                    CombatManager.SkillBar.ActivateSkill(0);
                    break;
                case InputAxis.SkillTwo:
                    CombatManager.SkillBar.ActivateSkill(1);
                    break;
                case InputAxis.SkillThree:
                    CombatManager.SkillBar.ActivateSkill(2);
                    break;
                case InputAxis.SkillFour:
                    CombatManager.SkillBar.ActivateSkill(3);
                    break;
            }
        }

        public void OnInputUp(InputAxis axis)
        {
            switch (axis)
            {
                case InputAxis.CancelCover:
                    break;
                case InputAxis.Submit:
                    break;
                case InputAxis.Fire:
                    break;
                case InputAxis.Flank:
                    break;
                case InputAxis.Reload:
                    break;
                case InputAxis.Vertical:
                    break;
                case InputAxis.Horizontal:
                    break;
                case InputAxis.Sprint:
                    MovementController.StopSprinting();
                    break;
            }
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
            if (axis == InputAxis.Horizontal)
            {
                MovementController.Dash(direction);
            }
        }

        public void CollectResourcesInRegion(Region region)
        {
            CollectResourcesAction.SetTargetRegion(region);
        }
    }
}