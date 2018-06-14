using System.Collections;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Combat.Ui;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI;
using UnityEngine;

namespace Game.Combat.Misc
{
    public abstract class CharacterCombat : MonoBehaviour
    {
        private const float DashForce = 300;
        private const float RecoilRecoveryRate = 2f;
        private const float TimeToStartRecovery = 0.5f;

        private const float KnockbackTime = 2f;

        private readonly Number Recoil = new Number(0, 0, 1f);
        private float _distanceToTarget = -1;

        private Vector2 _forceToadd = Vector2.zero;
        private float _recoveryTimer;

        private Rigidbody2D _rigidbody;
        protected ArmourController ArmourController;
        public HealthController HealthController = new HealthController();

        public bool IsDead;
        protected bool IsImmobilised;
        protected bool KnockedBack;

        protected bool FireImmune, SicknessImmune, DecayImmune;

        public float Speed;

        public float DistanceToTarget()
        {
            if (_distanceToTarget == -1) _distanceToTarget = Vector2.Distance(transform.position, GetTarget().transform.position);
            return _distanceToTarget;
        }

        public Vector3 Direction() => transform.up;

        public void Burn()
        {
            if (FireImmune) return;
            if (_burnTicks == 0) _burnDuration = 1;
            _burnTicks = ConditionTicksMax;
            if (this is PlayerCombat) PlayerUi.Instance().GetHealthController(this).StartBurning();
        }

        public bool IsBurning() => _burnTicks > 0;

        public void Decay()
        {
            if (DecayImmune) return;
            if (_decayTicks == 0) _decayDuration = 1;
            _decayTicks = ConditionTicksMax;
            if (this is PlayerCombat) PlayerUi.Instance().GetHealthController(this).StartBleeding();
        }

        public bool IsDecaying() => _decayTicks > 0;

        public void Sicken(int stacks = 1)
        {
            if (SicknessImmune) return;
            _sicknessTicks += stacks;
            if (_sicknessTicks >= SicknessTargetTicks)
            {
                HealthController.TakeDamage(HealthController.GetMaxHealth() / 4f);
                _sicknessTicks = 0;
            }

            _sicknessDuration = 0;
        }

        public bool IsSick() => _sicknessTicks > 0;

        public void Ram(CharacterCombat target, float ramForce)
        {
            Vector2 direction = (target.transform.position - transform.position).normalized;
            _forceToadd += direction * ramForce;
        }

        public void AddForce(Vector2 force)
        {
            _forceToadd += force;
        }

        public void ClearConditions()
        {
            _decayTicks = 0;
            _burnTicks = 0;
            _sicknessTicks = 0;
        }

        private void UpdateConditions()
        {
            if (_burnTicks > 0)
            {
                if (_burnDuration <= 0)
                {
                    _burnDuration = 1 - _burnDuration;
                    HealthController.TakeDamage(1);
                    --_burnTicks;
                }
                else
                {
                    _burnDuration -= Time.deltaTime;
                }
            }
            else
            {
                if (this is PlayerCombat) PlayerUi.Instance().GetHealthController(this).StopBurning();
            }

            if (_decayTicks > 0)
            {
                if (_burnDuration <= 0)
                {
                    _decayDuration = 1 - _decayDuration;
                    ArmourController.TakeDamage(1);
                    --_decayTicks;
                }
                else
                {
                    _decayDuration -= Time.deltaTime;
                }
            }
            else
            {
                if (this is PlayerCombat) PlayerUi.Instance().GetHealthController(this).StopBleeding();
            }

            if (_sicknessTicks > 0)
            {
                if (_sicknessDuration > SicknessDurationMax)
                {
                    _sicknessDuration = 0;
                    --_sicknessTicks;
                }
                else
                {
                    _sicknessDuration += Time.deltaTime;
                }
            }
        }

        private const int ConditionTicksMax = 5;
        private const float SicknessDurationMax = 5f;
        private const int SicknessTargetTicks = 10;
        private int _burnTicks, _decayTicks, _sicknessTicks;
        private float _burnDuration, _decayDuration, _sicknessDuration;

        public Cell CurrentCell() => PathingGrid.WorldToCellPosition(transform.position);

        protected void SetOwnedByEnemy(float speed)
        {
            Speed = speed;
        }

        public virtual void TakeDamage(Shot shot)
        {
            float armourProtection = ArmourController.GetCurrentArmour() / 10f;
            float armourDamage = shot.DamageDealt() * armourProtection;
            float healthDamage = shot.DamageDealt() - armourDamage;
            ArmourController.TakeDamage(Mathf.CeilToInt(armourDamage));
            HealthController.TakeDamage(Mathf.CeilToInt(healthDamage));
        }

        public virtual void Kill()
        {
            IsDead = true;
//            Destroy(CharacterController.gameObject);
        }

        public virtual void Knockback(Vector3 source, float force = 10f)
        {
            StartCoroutine(RecoverFromKnockback());
            Vector3 direction = (transform.position - source).normalized;
            _forceToadd = direction * force;
        }

        private IEnumerator RecoverFromKnockback()
        {
            KnockedBack = true;
            float age = 0;
            while (age < KnockbackTime)
            {
                age += Time.deltaTime;
                yield return null;
            }

            KnockedBack = false;
        }

        public virtual void ApplyShotEffects(Shot s)
        {
        }


        public abstract Weapon Weapon();

        //FIRING

        //CONDITIONS

        public void Immobilised(bool immobilised)
        {
            IsImmobilised = immobilised;
        }

        public virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public virtual void Update()
        {
            if (!CombatManager.InCombat()) return;
            if (GetTarget() != null) _distanceToTarget = Vector2.Distance(transform.position, GetTarget().transform.position);
            UpdateRecoil();
            UpdateConditions();
        }

        public void IncreaseRecoil()
        {
            float recoilLoss = Weapon().GetAttributeValue(AttributeType.Handling);
            recoilLoss = 100 - recoilLoss;
            recoilLoss /= 100;
            recoilLoss *= Moving() ? 2 : 1;
            Recoil.Increment(recoilLoss);
            _recoveryTimer = TimeToStartRecovery;
        }

        private bool Moving() => _rigidbody.velocity == Vector2.zero;

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

        public void FixedUpdate()
        {
            if (_rigidbody == null) Debug.Log(name);
            _rigidbody.AddForce(_forceToadd);
            _forceToadd = Vector2.zero;
        }

        protected virtual void Dash(Vector2 direction)
        {
            if (IsImmobilised) return;
            _forceToadd += direction * DashForce;
        }

        protected virtual void Move(Vector2 direction)
        {
            if (IsImmobilised) return;
            float speed = Speed;
            _forceToadd += direction * speed;
        }

        public abstract CharacterCombat GetTarget();
    }
}