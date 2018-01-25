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
using UnityEngine.SceneManagement;

namespace Game.Characters
{
    public abstract class Character : MyGameObject, IPersistenceTemplate
    {
        protected readonly DesolationInventory CharacterInventory;

//        protected Cooldown CoverCooldown;

        private long _timeAtLastFire;
        protected readonly Number ArmourLevel = new Number(0, 0, 10);

        public readonly HealthController HealthController;
        public readonly EquipmentController EquipmentController;
        public MovementController MovementController;

        protected bool InCover;
        public bool IsKnockedDown;

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
            Position.Min = float.MinValue;
            CharacterInventory = new DesolationInventory(name);
            HealthController = new HealthController(this);
            EquipmentController = new EquipmentController(this);
            Position.AddOnValueChange(a => CombatManager.CheckToEndCombatByFleeing());
//            SetCoverCooldown();
        }

        public virtual void Equip(GearItem gearItem)
        {
            EquipmentController.Equip(gearItem);
        }

        public virtual void OnHit(int damage, bool isCritical)
        {
            float armourModifier = 1 - 0.8f / ArmourLevel.Max * ArmourLevel.CurrentValue();
            damage = (int) (armourModifier * damage);
            damage = GetCoverProtection(damage);
            if (isCritical) HealthController.TakeCriticalDamage(damage);
            else HealthController.TakeDamage(damage);
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
            Shot shot = new Shot(target, this);
            shot.Fire();
            if (EquipmentController.Weapon().WeaponAttributes.Automatic)
            {
                _timeAtLastFire = Helper.TimeInMillis();
            }
            else
            {
                EquipmentController.Weapon().Cocked = false;
            }

            return shot;
        }

        protected virtual void Interrupt()
        {
        }

        //Only call from combatmanager
        public virtual void Update()
        {
            Burning.Update();
            Sickening.Update();
            Bleeding.Update();
        }

        public virtual void KnockDown()
        {
            Interrupt();
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