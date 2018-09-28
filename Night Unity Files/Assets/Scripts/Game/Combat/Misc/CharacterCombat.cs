using System.Collections.Generic;
using Facilitating.UIControllers;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Combat.Ui;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Combat.Misc
{
    [RequireComponent(typeof(MovementController))]
    public abstract class CharacterCombat : MonoBehaviour, ITakeDamageInterface
    {
        public WeaponAudioController WeaponAudio;

        private const float RecoilRecoveryRate = 2f;
        private const float TimeToStartRecovery = 0.5f;

        private readonly Number Recoil = new Number(0, 0, 1f);
        private float _distanceToTarget = -1;
        protected SpriteRenderer Sprite;

        private float _recoveryTimer;

        public ArmourController ArmourController;
        public HealthController HealthController = new HealthController();
        private CharacterCombat _target;
        public MovementController MovementController;
        private BloodSpatter _bloodSpatter;

        private bool _isDead;
        private const float SicknessDurationMax = 5f;
        private const int SicknessTargetTicks = 10;
        private readonly List<float> _burnTicks = new List<float>();
        private float _timeSinceLastBurn;
        private const float BurnTickDuration = 4f;
        protected int SicknessStacks;
        private float _sicknessDuration;
        public Shield Shield;
        private DamageSpriteFlash _spriteFlash;

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        protected float DistanceToTarget()
        {
            if (_distanceToTarget == -1) _distanceToTarget = Vector2.Distance(transform.position, GetTarget().transform.position);
            return _distanceToTarget;
        }

        public Vector3 Direction() => transform.up;

        public void Burn()
        {
            if (_timeSinceLastBurn < 1f) return;
            _timeSinceLastBurn = 0f;
            if (_burnTicks.Count == 0)
            {
                HealthBarController()?.StartBurning();
                if (this is EnemyBehaviour) PlayerCombat.Instance.Player.BrandManager.IncreaseBurnCount();
            }

            _burnTicks.Add(BurnTickDuration);
        }

        protected abstract UIHealthBarController HealthBarController();
        protected abstract UIArmourController ArmourBarController();

        public bool IsBurning() => _burnTicks.Count > 0;

        public void Decay()
        {
            TakeArmourDamage(GetDecayDamage());
        }

        public void Sicken(int stacks = 1)
        {
            SicknessStacks += stacks;
            if (SicknessStacks >= GetSicknessTargetTicks())
            {
                HealthController.TakeDamage(HealthController.GetMaxHealth() / 4f);
                if (this is EnemyBehaviour) PlayerCombat.Instance.Player.BrandManager.IncreaseSickenCount();
                SicknessStacks = 0;
            }

            _sicknessDuration = 0;
        }

        public bool IsSick() => SicknessStacks > 0;

        public void ClearConditions()
        {
            _burnTicks.Clear();
            SicknessStacks = 0;
        }

        protected virtual int GetBurnDamage()
        {
            int burnDamage = Mathf.FloorToInt(HealthController.GetMaxHealth() * 0.01f);
            if (burnDamage < 1) burnDamage = 1;
            return burnDamage;
        }

        protected virtual int GetDecayDamage()
        {
            int decayDamage = Mathf.FloorToInt(Armour.ArmourHealthUnit * 0.5f);
            if (decayDamage < 1) decayDamage = 1;
            return decayDamage;
        }

        protected virtual int GetSicknessTargetTicks()
        {
            return SicknessTargetTicks;
        }

        private void UpdateBurn()
        {
            _timeSinceLastBurn += Time.deltaTime;
            if (_burnTicks.Count > 0)
            {
                for (int i = _burnTicks.Count - 1; i >= 0; --i)
                {
                    float burnTick = _burnTicks[i];
                    float newBurnTick = burnTick - Time.deltaTime;
                    if (burnTick >= 3f && newBurnTick < 3f || burnTick >= 2f && newBurnTick < 2f || burnTick >= 1f && newBurnTick < 1f || burnTick > 0f && newBurnTick < 0f)
                    {
                        HealthController.TakeDamage(GetBurnDamage());
                    }

                    _burnTicks[i] = newBurnTick;
                    if (newBurnTick < 0f)
                    {
                        _burnTicks.RemoveAt(i);
                    }
                }
            }
            else
            {
                HealthBarController()?.StopBurning();
            }
        }

        private void UpdateSickness()
        {
            HealthBarController()?.SetSicknessLevel((float) SicknessStacks / GetSicknessTargetTicks());
            if (SicknessStacks <= 0) return;
            if (_sicknessDuration > SicknessDurationMax)
            {
                _sicknessDuration = 0;
                --SicknessStacks;
            }
            else
            {
                _sicknessDuration += Time.deltaTime;
            }
        }

        private void UpdateConditions()
        {
            UpdateBurn();
            UpdateSickness();
        }

        public void TakeArmourDamage(float damage)
        {
            ArmourController.TakeDamage(Mathf.CeilToInt(damage));
        }

        public Cell CurrentCell() => PathingGrid.WorldToCellPosition(transform.position);

        public virtual void TakeShotDamage(Shot shot)
        {
            _spriteFlash.FlashSprite();
            MovementController.KnockBack(shot.Direction(), shot.GetKnockBackForce());
            float armourProtection = ArmourController.GetCurrentProtection() / 10f;
            int armourDamage = Mathf.CeilToInt(shot.DamageDealt() * armourProtection);
            int healthDamage = shot.DamageDealt() - armourDamage;
            TakeArmourDamage(armourDamage);
            HealthController.TakeDamage(healthDamage);
            if (_bloodSpatter == null) return;
            _bloodSpatter.Spray(shot.Direction(), healthDamage);
            if (HealthController.GetCurrentHealth() != 0) return;
            LeafBehaviour.CreateLeaves(shot.Direction(), transform.position);
        }

        public bool IsDead()
        {
            return _isDead;
        }

        public virtual void TakeRawDamage(float damage, Vector2 direction)
        {
            float armourProtection = ArmourController.GetCurrentProtection() / 10f;
            float armourDamage = damage * armourProtection;
            float healthDamage = damage - armourDamage;
            TakeArmourDamage(armourDamage);
            HealthController.TakeDamage(Mathf.CeilToInt(healthDamage));
            _spriteFlash.FlashSprite();
            if (_bloodSpatter == null) return;
            _bloodSpatter.Spray(direction, healthDamage);
        }

        public virtual void TakeExplosionDamage(float damage, Vector2 origin)
        {
            _spriteFlash.FlashSprite();
            HealthController.TakeDamage(damage);
            Vector2 direction = ((Vector2) transform.position - origin).normalized;
            MovementController.KnockBack(direction, 50f / origin.Distance(transform.position));
        }

        public virtual void Kill()
        {
            _isDead = true;
            WeaponAudio.Destroy();
        }

        public virtual void ApplyShotEffects(Shot s)
        {
        }

        public abstract Weapon Weapon();

        public virtual void Awake()
        {
            Sprite = GetComponent<SpriteRenderer>();
            _spriteFlash = GetComponent<DamageSpriteFlash>();
            MovementController = GetComponent<MovementController>();
            _bloodSpatter = GetComponent<BloodSpatter>();
            WeaponAudio = gameObject.FindChildWithName<WeaponAudioController>("Weapon Audio");
            Shield = gameObject.FindChildWithName<Shield>("Shield");
        }

        public virtual void MyUpdate()
        {
            if (GetTarget() != null) _distanceToTarget = Vector2.Distance(transform.position, GetTarget().transform.position);
            UpdateRecoil();
            UpdateConditions();
        }

        public void IncreaseRecoil()
        {
            float recoilLoss = Weapon().GetAttributeValue(AttributeType.Handling);
            recoilLoss = 100 - recoilLoss;
            recoilLoss /= 100;
            recoilLoss *= MovementController.Moving() ? 2 : 1;
            Recoil.Increment(recoilLoss);
            _recoveryTimer = TimeToStartRecovery;
        }

        public virtual float GetAccuracyModifier() => Recoil.CurrentValue();

        private void UpdateRecoil()
        {
            if (_recoveryTimer > 0)
            {
                _recoveryTimer -= Time.deltaTime;
                return;
            }

            Recoil.Decrement(RecoilRecoveryRate * Time.deltaTime);
        }

        public void SetTarget(CharacterCombat character)
        {
            _target = character;
        }

        public virtual CharacterCombat GetTarget()
        {
            return _target;
        }
    }
}