using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Facilitating.Audio;
using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Combat;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using Game.World;
using SamsHelper;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.CooldownSystem;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Persistence;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Characters
{
    public abstract class Character : MyGameObject, IPersistenceTemplate
    {
        public readonly StateMachine<BaseCharacterAction> States = new StateMachine<BaseCharacterAction>();
        public readonly CharacterConditions Conditions;

        private readonly Dictionary<GearSubtype, GearItem> _equippedGear = new Dictionary<GearSubtype, GearItem>();
        protected readonly Inventory CharacterInventory;

        public readonly BaseAttributes BaseAttributes;

        private bool _sprinting;
        protected Cooldown CockingCooldown;
        protected Cooldown ReloadingCooldown;
        protected Cooldown DashCooldown;
        protected Cooldown KnockdownCooldown;
        protected Cooldown CoverCooldown;

        private const float KnockdownDuration = 3f;
        protected const float DashDuration = 2f;
        private long _timeAtLastFire;
        protected MyValue ArmourLevel = new MyValue();

        public readonly MyValue Rage = new MyValue(1, 0, 1);
        private static bool _rageActivated;

        public Action<Shot> OnFireAction;
        public bool Retaliate = false;

        public void IncreaseRage()
        {
            if (!_rageActivated)
            {
                Rage.Increment(0.05f);
            }
        }

        public bool RageActivated() => _rageActivated;

        public bool DecreaseRage()
        {
            float decreaseAmount = 0f;
            if (Rage.GetCurrentValue() < 1 && !_rageActivated)
            {
                decreaseAmount = -0.04f;
            }
            else if (_rageActivated)
            {
                decreaseAmount = -0.1f;
            }
            Rage.Increment(decreaseAmount * Time.deltaTime);
            return !Rage.ReachedMin();
        }

        private void EndRageMode()
        {
            if (!_rageActivated) return;
            _rageActivated = false;
            Weapon weapon = Weapon();
            weapon.WeaponAttributes.ReloadSpeed.RemoveModifier(-0.5f);
            weapon.WeaponAttributes.FireRate.RemoveModifier(0.5f);
        }

        public void TryStartRageMode()
        {
            if (Rage.GetCurrentValue() != 1) return;
            _rageActivated = true;
            Weapon weapon = Weapon();
            weapon.WeaponAttributes.ReloadSpeed.AddModifier(-0.5f);
            weapon.WeaponAttributes.FireRate.AddModifier(0.5f);
        }

        private enum CoverLevel
        {
            None,
            Partial,
            Total
        }

        private CoverLevel _coverLevel;

        protected Character(string name) : base(name, GameObjectType.Character)
        {
            CharacterInventory = new DesolationInventory(name);
            Conditions = new CharacterConditions();
            BaseAttributes = new BaseAttributes(this);
            foreach (GearSubtype gearSlot in Enum.GetValues(typeof(GearSubtype)))
            {
                _equippedGear[gearSlot] = null;
            }
            SetReloadCooldown();
            SetKnockdownCooldown();
            SetCoverCooldown();
            SetCockCooldown();
            SetDashCooldown();
            Rage.OnMin(EndRageMode);
        }

        public Condition GetCondition(ConditionType type)
        {
            return Conditions.Conditions[type];
        }

        public Weapon Weapon() => GetGearItem(GearSubtype.Weapon) as Weapon;
        public Armour Armour() => GetGearItem(GearSubtype.Armour) as Armour;
        public Accessory Accessory() => GetGearItem(GearSubtype.Accessory) as Accessory;

        public virtual void TakeDamage(Shot shot, int damage)
        {
            BaseAttributes.Strength.SetCurrentValue(BaseAttributes.Strength.GetCurrentValue() - damage);
            CombatManager.CombatUi.UpdateCharacterHealth(BaseAttributes.Strength);
            if (BaseAttributes.Strength.ReachedMin())
            {
                //TODO kill character
                return;
            }
            if (Retaliate) FireWeapon(shot?.Origin());
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

        public GearItem GetGearItem(GearSubtype type)
        {
            return _equippedGear.ContainsKey(type) ? _equippedGear[type] : null;
        }

        public Inventory Inventory()
        {
            return CharacterInventory;
        }

        public List<BaseCharacterAction> StatesAsList(bool includeInactiveStates)
        {
            return (from BaseCharacterAction s in States.StatesAsList() where s.IsStateVisible() || includeInactiveStates select s).ToList();
        }

        public virtual void Equip(GearItem gearItem)
        {
            Inventory previousInventory = gearItem.ParentInventory;
            if (!CharacterInventory.ContainsItem(gearItem))
            {
                gearItem.MoveTo(CharacterInventory);
            }
            GearItem previousEquipped = _equippedGear[gearItem.GetGearType()];
            if (previousEquipped != null)
            {
                previousEquipped.RemoveOwner();
                previousEquipped.Modifier.Remove(BaseAttributes);
                previousEquipped.Equipped = false;
                previousEquipped.MoveTo(previousInventory);
            }
            gearItem.SetOwner(this);
            gearItem.Equipped = true;
            _equippedGear[gearItem.GetGearType()] = gearItem;
            gearItem.Modifier.Apply(BaseAttributes);
        }

        //Cooldowns

        protected virtual void SetDashCooldown()
        {
            DashCooldown = CombatManager.CombatCooldowns.CreateCooldown(DashDuration);
        }

        protected virtual void SetCockCooldown()
        {
            CockingCooldown = CombatManager.CombatCooldowns.CreateCooldown();
            CockingCooldown.SetEndAction(() => { Weapon().Cocked = true; });
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
                Weapon().Cocked = true;
                Weapon().Reload(Inventory());
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
            BaseAttributes.Endurance.AddModifier(2);
            _sprinting = true;
        }

        public void StopSprinting()
        {
            if (!_sprinting) return;
            BaseAttributes.Endurance.RemoveModifier(2);
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
            float cockTime = Weapon().WeaponAttributes.FireRate.GetCalculatedValue();
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
            return !Weapon().Automatic && !Weapon().Cocked && !Weapon().Empty();
        }

        //RELOADING
        protected void TryReload()
        {
            if (Immobilised()) return;
            if (NeedsCocking() && !Weapon().Empty())
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
            if (Weapon().FullyLoaded()) return;
            if (Inventory().GetResourceQuantity(InventoryResourceType.Ammo) == 0) return;
            OnFireAction = null;
            Retaliate = false;
            float reloadSpeed = Weapon().GetAttributeValue(AttributeType.ReloadSpeed);
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
            return !Immobilised() && Weapon().Cocked && !Weapon().Empty() && FireRateElapsedTimeMet();
        }

        private bool FireRateElapsedTimeMet()
        {
            long timeElapsed = Helper.TimeInMillis() - _timeAtLastFire;
            long targetTime = (long) (1f / Weapon().GetAttributeValue(AttributeType.FireRate) * 1000);
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
            if (normalShot.DidHit())
            {
                IncreaseRage();
            }
            if (Weapon().Automatic)
            {
                _timeAtLastFire = Helper.TimeInMillis();
            }
            else
            {
                Weapon().Cocked = false;
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