using System.Xml;
using Facilitating.Persistence;
using Game.Characters.Player;
using Game.Combat;
using Game.Combat.Skills;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Game.Characters
{
    public abstract class Character : MyGameObject, IPersistenceTemplate, ICombatListener
    {
        protected readonly DesolationInventory CharacterInventory;

        private long _timeAtLastFire;
        //todo implement armour
        public readonly Number ArmourLevel = new Number(0, 0, 10);

        public readonly HealthController HealthController;
        public readonly EquipmentController EquipmentController;
        public MovementController MovementController;

        protected bool InCover;
        public bool IsKnockedDown;
        public bool IsDead;

        public Bleed Bleeding;
        public Burn Burn;
        public Sickness Sick;

        public readonly Number Position = new Number();

        public Direction FacingDirection;

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
            Position.Min = float.MinValue;
            Position.AddOnValueChange(a => CharacterPositionManager.UpdatePlayerDirection());
            CharacterInventory = new DesolationInventory(name);
            HealthController = new HealthController(this);
            EquipmentController = new EquipmentController(this);
        }

        public virtual void Equip(GearItem gearItem)
        {
            ((Weapon) gearItem)?.Reload(Inventory());
            EquipmentController.Equip(gearItem);
        }

        public virtual void OnHit(int damage)
        {
            if (InCover) return;
            //todo pierce through cover?
            float armourModifier = 1 - 0.8f / ArmourLevel.Max * ArmourLevel.CurrentValue();
            damage = (int) (armourModifier * damage);
            HealthController.TakeDamage(damage);
        }

        public virtual void OnMiss()
        {
        }

        public abstract void Kill();

        public void Load(XmlNode doc, PersistenceType saveType)
        {
            Name = doc.SelectSingleNode("Name").InnerText;
        }

        public DesolationInventory Inventory()
        {
            return CharacterInventory;
        }


        //COVER
        public virtual void TakeCover()
        {
            if (Immobilised() || InCover) return;
            InCover = true;
        }

        public virtual void LeaveCover()
        {
            if (Immobilised() || !InCover) return;
            InCover = false;
        }

        public virtual bool Immobilised()
        {
            return IsKnockedDown;
        }

        //FIRING
        private bool CanFire()
        {
            Weapon weapon = EquipmentController.Weapon();
            return !Immobilised() && !weapon.Empty() && FireRateElapsedTimeMet() && !InCover;
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
            Assert.IsNotNull(target);
            Shot shot = new Shot(target, this);
            if(FacingDirection == target.FacingDirection) shot.GuaranteeCritical();
            _timeAtLastFire = Helper.TimeInMillis();
            return shot;
        }

        protected virtual void Interrupt()
        {
        }

        //Only call from combatmanager
        public virtual void UpdateCombat()
        {
            Burn.Update();
            Sick.Update();
            Bleeding.Update();
        }

        protected virtual void KnockDown()
        {
            Interrupt();
            LeaveCover();
            IsKnockedDown = true;
        }

        public void Knockback(float knockbackDistance)
        {
            MovementController.KnockBack(knockbackDistance);
            KnockDown();
        }

        //CONDITIONS

        protected virtual void SetConditions()
        {
            Bleeding = new Bleed(this);
            Burn = new Burn(this);
            Sick = new Sickness(this);
        }

        public virtual void EnterCombat()
        {
            HealthController.EnterCombat();
        }

        public virtual void ExitCombat()
        {
            HealthController.ExitCombat();
            Burn.Clear();
            Bleeding.Clear();
            Sick.Clear();
        }
    }
}