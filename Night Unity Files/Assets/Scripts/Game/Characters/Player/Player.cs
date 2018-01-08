using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Audio;
using Facilitating.Persistence;
using Game.Characters.Attributes;
using Game.Characters.CharacterActions;
using Game.Combat;
using Game.Combat.Enemies;
using Game.Combat.Skills;
using Game.Gear.Weapons;
using Game.World.Region;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Input;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.InventoryUI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace Game.Characters
{
    public class Player : Character, IInputListener
    {
        public readonly StateMachine States = new StateMachine();
        public readonly TraitLoader.Trait CharacterClass, CharacterTrait;
        public int DistanceFromHome;
        public CharacterView CharacterView;
        public readonly SurvivalAttributes SurvivalAttributes;
        public readonly MyValue Energy = new MyValue();

        public CollectResources CollectResourcesAction;
        public Sleep SleepAction;
        public Idle IdleAction;
        public Travel TravelAction;
        public Return ReturnAction;
        public CraftAmmo CraftAmmoAction;
        public CharacterActions.Combat CombatAction;
        public LightFire LightFireAction;

        //Create Character in code only- no view section, no references to objects in the scene
        public Player(string name, TraitLoader.Trait characterClass, TraitLoader.Trait characterTrait) : base(name)
        {
            SurvivalAttributes = new SurvivalAttributes(this);
            CharacterClass = characterClass;
            CharacterTrait = characterTrait;
            CharacterInventory.MaxWeight = 50;
            BaseAttributes.Endurance.AddOnValueChange(a => Energy.Max = a.CurrentValue());
            Energy.OnMin(Sleep);
        }

        public override XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            base.Save(doc, saveType);
            SaveController.CreateNodeAndAppend("Class", doc, CharacterClass.Name);
            SaveController.CreateNodeAndAppend("Trait", doc, CharacterTrait.Name);
            SaveController.CreateNodeAndAppend("Distance", doc, DistanceFromHome);
            SaveController.CreateNodeAndAppend("Energy", doc, Energy.CurrentValue());
            SurvivalAttributes.Save(doc, saveType);
            return doc;
        }

        protected override float GetSpeedModifier()
        {
            float walkSpeed = 1f + BaseAttributes.Endurance.CurrentValue() / 20f;
            return walkSpeed * Time.deltaTime;
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
            int characterWeight = 5 + SurvivalAttributes.Weight;
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
                    CombatManager.CombatUi.SkillBar.BindSkill(3, new Skill.Retribution(this));
                    CombatManager.CombatUi.SkillBar.BindSkill(4, new Skill.Revenge(this));
                    break;
                case WeaponType.Rifle:
                    CombatManager.CombatUi.SkillBar.BindSkill(3, new Skill.PiercingShot(this));
                    CombatManager.CombatUi.SkillBar.BindSkill(4, new Skill.FullBlast(this));
                    break;
                case WeaponType.Shotgun:
                    CombatManager.CombatUi.SkillBar.BindSkill(3, new Skill.LegSweep(this));
                    CombatManager.CombatUi.SkillBar.BindSkill(4, new Skill.BulletCloud(this));
                    break;
                case WeaponType.SMG:
                    CombatManager.CombatUi.SkillBar.BindSkill(3, new Skill.DoubleUp(this));
                    CombatManager.CombatUi.SkillBar.BindSkill(4, new Skill.Splinter(this));
                    break;
                case WeaponType.LMG:
                    CombatManager.CombatUi.SkillBar.BindSkill(3, new Skill.TopUp(this));
                    CombatManager.CombatUi.SkillBar.BindSkill(4, new Skill.HeavyLead(this));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //COOLDOWNS

        protected override void SetDashCooldown()
        {
            base.SetDashCooldown();
            DashCooldown.SetController(CombatManager.CombatUi.DashCooldownController);
        }

        protected override void SetCockCooldown()
        {
            base.SetCockCooldown();
            CockingCooldown.SetEndAction(() =>
            {
                EquipmentController.Weapon().Cocked = true;
                UpdateMagazineUi();
            });
            CockingCooldown.SetDuringAction(f => CombatManager.CombatUi.UpdateReloadTime(f));
        }

        protected override void SetKnockdownCooldown()
        {
            base.SetKnockdownCooldown();
            KnockdownCooldown.SetEndAction(() => { CombatManager.CombatUi.ConditionsText.text = ""; });
            KnockdownCooldown.SetDuringAction(f => CombatManager.CombatUi.ConditionsText.text = "Knocked down! " + Helper.Round(f, 1) + "s");
        }

        protected override void SetReloadCooldown()
        {
            base.SetReloadCooldown();
            ReloadingCooldown.SetEndAction(() =>
            {
                EquipmentController.Weapon().Cocked = true;
                EquipmentController.Weapon().Reload(Inventory());
                UpdateMagazineUi();
            });
            ReloadingCooldown.SetDuringAction(f => CombatManager.CombatUi.UpdateReloadTime(f));
        }

        //FIRING
        public override Shot FireWeapon(Character target)
        {
            Shot shot = base.FireWeapon(target);
            UpdateMagazineUi();
            return shot;
        }

        //MISC

        public override void Interrupt()
        {
            base.Interrupt();
            UpdateMagazineUi();
        }

        public void UpdateMagazineUi()
        {
            string magazineMessage = "";
            if (Weapon().GetAmmoAvailable() == 0) magazineMessage = "NO AMMO";
            if (!EquipmentController.Weapon().Cocked) magazineMessage = "EJECT CARTRIDGE";
            else if (EquipmentController.Weapon().Empty()) magazineMessage = "RELOAD";

            if (magazineMessage == "")
            {
                CombatManager.CombatUi.UpdateMagazine(EquipmentController.Weapon().GetRemainingAmmo());
            }
            else
            {
                CombatManager.CombatUi.EmptyMagazine();
                CombatManager.CombatUi.SetMagazineText(magazineMessage);
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
                case InputAxis.Flank:
                    Flank();
                    break;
                case InputAxis.Reload:
                    TryReload();
                    break;
                case InputAxis.Vertical:
                    break;
                case InputAxis.Horizontal:
                    Move(direction);
                    break;
                case InputAxis.Sprint:
                    StartSprinting();
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
                    StopSprinting();
                    break;
            }
        }

        public void OnDoubleTap(InputAxis axis, float direction)
        {
            if (axis == InputAxis.Horizontal)
            {
                Dash(direction);
            }
        }

        public void CollectResourcesInRegion(Region region)
        {
            CollectResourcesAction.SetTargetRegion(region);
        }
    }
}