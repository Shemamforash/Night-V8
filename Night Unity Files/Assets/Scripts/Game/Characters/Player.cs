﻿using System;
using Game.Characters.Attributes;
using Game.Characters.CharacterActions;
using Game.Combat;
using Game.Combat.Skills;
using Game.Gear.Weapons;
using Game.World.Region;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Input;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Characters
{
    public class Player : Character, IInputListener
    {
        public readonly TraitLoader.Trait CharacterClass, CharacterTrait;
        public Region CurrentRegion;
        public CharacterView CharacterView;
        public readonly SurvivalAttributes SurvivalAttributes;
        public Skill ClassSkillOne;
        public Skill ClassSkillTwo;
        public Skill WeaponSkillOne;
        public Skill WeaponSkillTwo;

        //Create Character in code only- no view section, no references to objects in the scene
        public Player(string name, TraitLoader.Trait characterClass, TraitLoader.Trait characterTrait) : base(name)
        {
            SurvivalAttributes = new SurvivalAttributes(this);
            CharacterClass = characterClass;
            CharacterTrait = characterTrait;
            CharacterInventory.MaxWeight = 50;
            InputHandler.RegisterInputListener(this);
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

        private void AddStates()
        {
            States.AddState(new CollectResources(this));
            States.AddState(new CharacterActions.Combat(this));
            States.AddState(new Sleep(this));
            States.AddState(new Idle(this));
            States.AddState(new PrepareTravel(this));
            States.AddState(new Travel(this));
            States.AddState(new Return(this));
            States.AddState(new LightFire(this));
            States.SetDefaultState("Idle");
            CharacterView.UpdateActionUi();
        }

        private bool IsOverburdened()
        {
            return CharacterInventory.Weight > BaseAttributes.Strength.GetCurrentValue();
        }

        private void Tire(int amount)
        {
            BaseAttributes.Endurance.SetCurrentValue(BaseAttributes.Endurance.GetCurrentValue() - (IsOverburdened() ? amount * 2 : amount));
            CheckEnduranceZero();
        }

        private void CheckEnduranceZero()
        {
            if (!BaseAttributes.Endurance.ReachedMin()) return;
            BaseCharacterAction action = States.GetCurrentState() as BaseCharacterAction;
            action.Interrupt();
            Sleep sleepAction = States.NavigateToState("Sleep") as Sleep;
            sleepAction.SetDuration((int) (BaseAttributes.Endurance.Max / 5f));
            sleepAction.SetStateTransitionTarget(action.Name);
            sleepAction.AddOnExit(() => { action.Resume(); });
            sleepAction.Start();
        }

        public void Rest(int amount)
        {
            BaseAttributes.Endurance.SetCurrentValue(BaseAttributes.Endurance.GetCurrentValue() + amount);
            if (!BaseAttributes.Endurance.ReachedMax()) return;
            if (CurrentRegion == null)
            {
                States.NavigateToState("Idle");
            }
        }

        public void Travel()
        {
            Tire(CalculateEnduranceCostForDistance(1));
        }

        public int CalculateTotalWeight()
        {
            int characterWeight = 5 + (int) SurvivalAttributes.Weight;
            int inventoryWeight = (int) (CharacterInventory.Weight / 10);
            return characterWeight + inventoryWeight;
        }

        public int CalculateEnduranceCostForDistance(int distance)
        {
            return distance * CalculateTotalWeight();
        }

        public override void Equip(GearItem gearItem)
        {
            base.Equip(gearItem);
            switch (gearItem.GetGearType())
            {
                case GearSubtype.Weapon:
                    SwapWeaponSkills((Weapon) gearItem);
                    CharacterView?.WeaponGearUi.Update(gearItem);
                    break;
                case GearSubtype.Armour:
                    CharacterView?.ArmourGearUi.Update(gearItem);
                    break;
                case GearSubtype.Accessory:
                    CharacterView?.AccessoryGearUi.Update(gearItem);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SwapWeaponSkills(Weapon weapon)
        {
            switch (weapon.WeaponClass.Type)
            {
                case WeaponType.Pistol:
                    break;
                case WeaponType.Rifle:
                    WeaponSkillOne?.Cancel();
                    WeaponSkillOne = new Skill.PiercingShot(this);
                    WeaponSkillOne.SetController(CombatManager.CombatUi.WeaponSkillOneCooldownController);
                    break;
                case WeaponType.Shotgun:
                    break;
                case WeaponType.SMG:
                    break;
                case WeaponType.LMG:
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
                Weapon().Cocked = true;
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
                Weapon().Cocked = true;
                Weapon().Reload(Inventory());
                UpdateMagazineUi();
            });
            ReloadingCooldown.SetDuringAction(f => CombatManager.CombatUi.UpdateReloadTime(f));
        }

        //FIRING

        protected override void FireWeapon()
        {
            base.FireWeapon();
            UpdateMagazineUi();
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
                    TryStartRageMode();
                    break;
                case InputAxis.Fire:
                    FireWeapon();
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
                case InputAxis.SkillOne:
                    break;
                case InputAxis.SkillTwo:
                    break;
                case InputAxis.SkillThree:
                    WeaponSkillOne?.Activate();
                    break;
                case InputAxis.SkillFour:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, null);
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
    }
}