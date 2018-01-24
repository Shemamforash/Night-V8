using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Xml;
using Assets;
using Facilitating.Audio;
using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Characters.Player;
using Game.Combat;
using Game.Combat.Enemies;
using Game.Combat.Enemies.EnemyTypes;
using Game.Combat.Skills;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public abstract class Character : MyGameObject, IPersistenceTemplate
    {
        protected readonly DesolationInventory CharacterInventory;
        protected Cooldown CockingCooldown;
        protected Cooldown ReloadingCooldown;

//        protected Cooldown CoverCooldown;

        private long _timeAtLastFire;
        protected readonly Number ArmourLevel = new Number(0, 0, 10);

        public Action<Shot> OnFireAction;
        public bool Retaliate;

        public readonly HealthController HealthController;
        public readonly EquipmentController EquipmentController;
        public MovementController MovementController;

        protected bool InCover;
        public bool KnockedDown;
        protected Action TakeCoverAction, LeaveCoverAction;

        protected Condition Bleeding, Burning, Sickening;

        public readonly Number Position = new Number();

        public virtual XmlNode Save(XmlNode doc, PersistenceType saveType)
        {
            SaveController.CreateNodeAndAppend("Name", doc, Name);
            EquipmentController.Save(doc, saveType);
            Inventory().Save(doc, saveType);
            return doc;
        }

        public Weapon Weapon()
        {
            return EquipmentController.Weapon();
        }

        protected Character(string name) : base(name, GameObjectType.Character)
        {
            CharacterInventory = new DesolationInventory(name);
            HealthController = new HealthController(this);
            EquipmentController = new EquipmentController(this);
            SetReloadCooldown();
//            SetCoverCooldown();
            SetCockCooldown();
        }

        public virtual void Equip(GearItem gearItem)
        {
            EquipmentController.Equip(gearItem);
        }

        public void OnHit(int damage, bool isCritical)
        {
            float armourModifier = 1 - 0.8f / ArmourLevel.Max * ArmourLevel.CurrentValue();
            damage = (int) (armourModifier * damage);
            damage = GetCoverProtection(damage);
            if (isCritical) HealthController.TakeCriticalDamage(damage);
            else HealthController.TakeDamage(damage);
        }
        
        public void OnHit(Shot shot, int damage, bool isCritical)
        {
           OnHit(damage, isCritical);
            if (Retaliate) FireWeapon(shot?.Origin());
        }

        private int GetCoverProtection(int damage)
        {
            if (InCover) return (int) (damage * 0.5f);
            return damage;
        }

        public virtual void OnMiss()
        {
        }

        public virtual void Kill()
        {
            if (SceneManager.GetActiveScene().name == "Game") WorldState.HomeInventory().RemoveItem(this);
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            Name = doc.SelectSingleNode("Name").InnerText;
        }

        public DesolationInventory Inventory()
        {
            return CharacterInventory;
        }

        //Cooldowns

        private void SetCockCooldown()
        {
            CockingCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            CockingCooldown.SetEndAction(() => { EquipmentController.Weapon().Cocked = true; });
        }


        private void SetReloadCooldown()
        {
            ReloadingCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            ReloadingCooldown.SetEndAction(() =>
            {
                EquipmentController.Weapon().Cocked = true;
                EquipmentController.Weapon().Reload(Inventory());
            });
        }

        //COVER
        public void TakeCover()
        {
            if (Immobilised() || InCover) return;
            InCover = true;
            TakeCoverAction?.Invoke();
        }

        public void LeaveCover()
        {
            if (Immobilised() || !InCover) return;
            InCover = false;
            LeaveCoverAction?.Invoke();
        }

        //COCKING
        public void CockWeapon()
        {
            if (Immobilised()) return;
            if (!CockingCooldown.Finished()) return;
            float cockTime = EquipmentController.Weapon().WeaponAttributes.GetCalculatedValue(AttributeType.FireRate);
            GunFire.Cock(cockTime);
            CockingCooldown.Duration = cockTime;
            CockingCooldown.Start();
        }

        private void StopCocking()
        {
            if (CockingCooldown == null || CockingCooldown.Finished()) return;
            Debug.Log("stopped cocking");
            CockingCooldown.Cancel();
        }

        protected bool NeedsCocking()
        {
            Weapon weapon = EquipmentController.Weapon();
            bool automatic = weapon.WeaponAttributes.Automatic;
            return !automatic && !weapon.Cocked && !weapon.Empty();
        }

        //RELOADING
        protected void TryReload()
        {
            if (Immobilised()) return;
            if (NeedsCocking())
            {
                CockWeapon();
                return;
            }
            ReloadWeapon();
        }

        private void ReloadWeapon()
        {
            if (Immobilised()) return;
            if (ReloadingCooldown.Running()) return;
            if (EquipmentController.Weapon().FullyLoaded()) return;
            if (Weapon().GetRemainingMagazines() == 0) return;
            OnFireAction = null;
            Retaliate = false;
            float reloadSpeed = EquipmentController.Weapon().GetAttributeValue(AttributeType.ReloadSpeed);
            UIMagazineController.EmptyMagazine();
            ReloadingCooldown.Duration = reloadSpeed;
            ReloadingCooldown.Start();
        }

        private void StopReloading()
        {
            if (ReloadingCooldown == null || ReloadingCooldown.Finished()) return;
            ReloadingCooldown.Cancel();
        }

        //FIRING
        public bool CanFire()
        {
            Weapon weapon = EquipmentController.Weapon();
            return !Immobilised() && weapon.Cocked && !weapon.Empty() && FireRateElapsedTimeMet() && !InCover;
        }

        private bool FireRateElapsedTimeMet()
        {
            long timeElapsed = Helper.TimeInMillis() - _timeAtLastFire;
            long targetTime = (long) (1f / EquipmentController.Weapon().GetAttributeValue(AttributeType.FireRate) * 1000);
            return !(timeElapsed < targetTime);
        }

        protected virtual Shot FireWeapon(Character target)
        {
            if (!CanFire()) return null;
            if (target == null) target = CombatManager.GetTarget(this);
            Shot normalShot = new Shot(target, this);
            OnFireAction?.Invoke(normalShot);
            normalShot.Fire();
            if (EquipmentController.Weapon().WeaponAttributes.Automatic)
            {
                _timeAtLastFire = Helper.TimeInMillis();
            }
            else
            {
                EquipmentController.Weapon().Cocked = false;
            }

            return normalShot;
        }

        //Only call from combatmanager
        public virtual void Update()
        {
            Burning.Update();
            Sickening.Update();
            Bleeding.Update();
        }

        //MISC

        protected virtual void Interrupt()
        {
            StopCocking();
            StopReloading();
        }

        public bool Immobilised()
        {
            return ReloadingCooldown.Running() || CockingCooldown.Running() || KnockedDown;
        }

        public virtual void KnockDown()
        {
            Interrupt();
            KnockedDown = true;
        }

        public void Knockback(float knockbackDistance)
        {
            Interrupt();
            MovementController.KnockBack(knockbackDistance);
            KnockDown();
        }

        //CONDITIONS

        protected virtual void SetConditions()
        {
            Bleeding = new Bleed(this);
            Burning = new Burn(this);
            Sickening = new Sickness(this);
        }

        public void AddBleedStack()
        {
            Bleeding.AddStack();
        }

        public void AddSicknessStack()
        {
            Sickening.AddStack();
        }

        public void AddBurnStack()
        {
            Burning.AddStack();
        }
    }
}