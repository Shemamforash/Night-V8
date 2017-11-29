using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Audio;
using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Combat;
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
using Random = UnityEngine.Random;

namespace Game.Characters
{
    public abstract class Character : MyGameObject, IPersistenceTemplate
    {
        protected readonly DesolationInventory CharacterInventory;
        public readonly BaseAttributes BaseAttributes;
        protected Cooldown CockingCooldown;
        protected Cooldown ReloadingCooldown;
        protected Cooldown DashCooldown;
        protected Cooldown KnockdownCooldown;
        protected Cooldown CoverCooldown;

        private bool _sprinting;
        private const float KnockdownDuration = 3f;
        protected const float DashDuration = 2f;
        private long _timeAtLastFire;
        protected MyValue ArmourLevel = new MyValue(0,0,10);

        public Action<Shot> OnFireAction;
        public bool Retaliate;

        public RageController RageController;
        public HealthController HealthController;
        public EquipmentController EquipmentController;
        private AttributeModifier _sprintModifier = new AttributeModifier();
        
        private enum CoverLevel
        {
            None,
            Partial,
            Total
        }

        private CoverLevel _coverLevel;

        public Weapon Weapon()
        {
            return EquipmentController.Weapon();
        }
        
        protected Character(string name) : base(name, GameObjectType.Character)
        {
            _sprintModifier.SetMultiplicative(2);
            CharacterInventory = new DesolationInventory(name);
            BaseAttributes = new BaseAttributes(this);
            
            _sprintModifier.AddTargetAttribute(BaseAttributes.Endurance);

            RageController = new RageController(this);
            HealthController = new HealthController(this);
            EquipmentController = new EquipmentController(this);
            SetReloadCooldown();
            SetKnockdownCooldown();
            SetCoverCooldown();
            SetCockCooldown();
            SetDashCooldown();
        }

        public virtual void Equip(GearItem gearItem)
        {
            EquipmentController.Equip(gearItem);
        }

        public void OnHit(Shot shot, int damage, bool isCritical)
        {
            float armourModifier = 1- 0.8f / ArmourLevel.Max * ArmourLevel.CurrentValue();
            damage = (int) (armourModifier * damage);
            damage = GetCoverProtection(damage);
            if(isCritical) HealthController.TakeCriticalDamage(damage);
            else HealthController.TakeDamage(damage);
            if (Retaliate) FireWeapon(shot?.Origin());
        }
        
        private int GetCoverProtection(int damage)
        {
            bool protectedByCover = Random.Range(0, 2) == 0;
            if (protectedByCover)
            {
                if (InCover()) return (int) (damage * 0.5f);
                if (InPartialCover()) return (int) (damage * 0.75);
            }
            return damage;
        }

        public virtual void OnMiss()
        {
            
        }
        
        public virtual void Kill()
        {
            WorldState.HomeInventory().RemoveItem(this);
        }

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            Name = doc.SelectSingleNode("Name").InnerText;
            XmlNode attributesNode = doc.SelectSingleNode("Attributes");
            BaseAttributes.Load(attributesNode, saveType);
        }

        public void Save(XmlNode doc, PersistenceType saveType)
        {
            SaveController.CreateNodeAndAppend("Name", doc, Name);
            XmlNode attributesNode = SaveController.CreateNodeAndAppend("Attributes", doc);
            BaseAttributes.Save(attributesNode, saveType);
        }

        public DesolationInventory Inventory()
        {
            return CharacterInventory;
        }

        //Cooldowns

        protected virtual void SetDashCooldown()
        {
            DashCooldown = CombatManager.CombatCooldowns.CreateCooldown(DashDuration);
        }

        protected virtual void SetCockCooldown()
        {
            CockingCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            CockingCooldown.SetEndAction(() => { EquipmentController.Weapon().Cocked = true; });
        }

        protected virtual void SetKnockdownCooldown()
        {
            KnockdownCooldown = CombatManager.CombatCooldowns.CreateCooldown(KnockdownDuration);
        }

        protected virtual void SetCoverCooldown()
        {
            CoverCooldown = CombatManager.CombatCooldowns.CreateCooldown(2f);
            CoverCooldown.SetEndAction(() =>
            {
                _coverLevel = CoverLevel.Total;
                CombatManager.TakeCover(this);
            });
        }

        protected virtual void SetReloadCooldown()
        {
            ReloadingCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            ReloadingCooldown.SetEndAction(() =>
            {
               EquipmentController.Weapon().Cocked = true;
               EquipmentController.Weapon().Reload(Inventory());
            });
        }

        //MOVEMENT

        protected abstract float GetSpeedModifier();

        public void MoveForward()
        {
            if (Immobilised()) return;
            LeaveCover();
            CombatManager.ReduceDistance(this, GetSpeedModifier());
        }

        public void MoveBackward()
        {
            if (Immobilised()) return;
            LeaveCover();
            CombatManager.IncreaseDistance(this, GetSpeedModifier());
        }

        protected void Move(float direction)
        {
            if (direction > 0)
            {
                MoveForward();
            }
            else
            {
                MoveBackward();
            }
        }

        //SPRINTING

        public void StartSprinting()
        {
            if (_sprinting) return;
            _sprintModifier.Apply();
            _sprinting = true;
        }

        public void StopSprinting()
        {
            if (!_sprinting) return;
            _sprintModifier.Remove();
            _sprinting = false;
        }

//        public void SetFlanked()
//        {
//            _coverLevel = CoverLevel.Partial;
//        }

        //COVER
        public bool InCover()
        {
            return _coverLevel == CoverLevel.Total;
        }

        public void TakeCover()
        {
            if (Immobilised()) return;
            CoverCooldown.Start();
        }

        public void LeaveCover()
        {
            if (Immobilised()) return;
            if (!InCover()) return;
            _coverLevel = CoverLevel.None;
            CombatManager.LeaveCover(this);
        }

        public bool InPartialCover()
        {
            return _coverLevel == CoverLevel.Partial;
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
            if (Inventory().GetResourceQuantity(InventoryResourceType.Ammo) == 0) return;
            OnFireAction = null;
            Retaliate = false;
            float reloadSpeed = EquipmentController.Weapon().GetAttributeValue(AttributeType.ReloadSpeed);
            CombatManager.CombatUi.EmptyMagazine();
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
            return !Immobilised() && weapon.Cocked && !weapon.Empty() && FireRateElapsedTimeMet();
        }

        private bool FireRateElapsedTimeMet()
        {
            long timeElapsed = Helper.TimeInMillis() - _timeAtLastFire;
            long targetTime = (long) (1f / EquipmentController.Weapon().GetAttributeValue(AttributeType.FireRate) * 1000);
            return !(timeElapsed < targetTime);
        }

        //Returns true if target hit
        public virtual Shot FireWeapon(Character target)
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

        //DASHING

        protected void Dash(float direction)
        {
            if (direction < 0)
            {
                DashBackward();
                return;
            }
            DashForward();
        }

        private bool CanDash()
        {
            return DashCooldown.Finished();
        }

        private float _dashDistance = 5f;

        private void DashForward()
        {
            if (Immobilised()) return;
            if (!CanDash()) return;
            CombatManager.ReduceDistance(this, _dashDistance);
            DashCooldown.Start();
        }

        private void DashBackward()
        {
            if (Immobilised()) return;
            if (!CanDash()) return;
            CombatManager.IncreaseDistance(this, _dashDistance);
            DashCooldown.Start();
        }

        //MISC

        public virtual void Interrupt()
        {
            StopCocking();
            StopReloading();
            StopSprinting();
        }

        protected void Flank()
        {
            if (Immobilised()) return;
            CombatManager.Flank(this);
        }

        protected bool Immobilised()
        {
            return ReloadingCooldown.Running() || CockingCooldown.Running() || KnockdownCooldown.Running() || CoverCooldown.Running();
        }

        public void KnockDown()
        {
            if (KnockdownCooldown.Running()) return;
            KnockdownCooldown.Start();
            Interrupt();
        }

        public void Knockback(float knockbackDistance)
        {
            Interrupt();
            CombatManager.IncreaseDistance(this, knockbackDistance);
        }
    }
}