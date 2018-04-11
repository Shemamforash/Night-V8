using System.Collections;
using Game.Combat.Generation;
using Game.Gear.Armour;
using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.ReactiveUI;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Combat.Misc
{
    public abstract class CharacterCombat : MonoBehaviour
    {
        private const int SprintModifier = 2;
        private const float DashForce = 300;
        private const float RecoilRecoveryRate = 0.02f;
        private const float TimeToStartRecovery = 0.5f;

        private const float KnockbackTime = 2f;

        private readonly Number Recoil = new Number(0, 0, 1f);
        private Cell _currentCell;
        private float _distanceToTarget = -1;

        private Vector2 _forceToadd = Vector2.zero;
        private float _recoveryTimer;

        private Rigidbody2D _rigidbody;
        protected ArmourController ArmourController;
        public HealthController HealthController = new HealthController();

        public bool IsDead;
        protected bool IsImmobilised;
        protected bool KnockedBack;

        public float Speed;
        private bool Sprinting;

        public float DistanceToTarget()
        {
            if (_distanceToTarget == -1) _distanceToTarget = Vector2.Distance(transform.position, GetTarget().transform.position);
            return _distanceToTarget;
        }

        public void Burn()
        {
            if (_burnTicksRemaining == 0) _burnTime = 1;
            _burnTicksRemaining = ConditionTicksMax;
        }

        public void Bleed()
        {
            if (_bleedTicksRemaining == 0) _bleedTime = 1;
            _bleedTicksRemaining = ConditionTicksMax;
        }

        private void UpdateConditions()
        {
            if (_burnTicksRemaining > 0)
            {
                if (_burnTime <= 0)
                {
                    _burnTime = 1 - _burnTime;
                    ArmourController.TakeDamage(1);
                    --_burnTicksRemaining;
                }
                else
                {
                    _burnTime -= Time.deltaTime;
                }
            }
            if (_bleedTicksRemaining > 0)
            {
                if (_burnTime <= 0)
                {
                    _bleedTime = 1 - _bleedTime;
                    HealthController.TakeDamage(1);
                    --_bleedTicksRemaining;
                }
                else
                {
                    _bleedTime -= Time.deltaTime;
                }
            }
        }

        private const int ConditionTicksMax = 5;
        private int _burnTicksRemaining, _bleedTicksRemaining;
        private float _burnTime, _bleedTime;

        public Cell CurrentCell()
        {
            return _currentCell == null ? PathingGrid.Instance().PositionToCell(transform.position) : _currentCell;
        }

        protected void SetOwnedByEnemy(float speed)
        {
            Speed = speed;
        }

        public virtual void TakeDamage(Shot shot)
        {
            float chanceToAbsorbDamage = shot.DidPierce() ? 0 : ArmourController.GetCurrentArmour() / 10f;
            if (chanceToAbsorbDamage >= Random.Range(0f, 1f))
                ArmourController.TakeDamage(shot.DamageDealt());
            else
                HealthController.TakeDamage(shot.DamageDealt());
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

        public abstract Weapon Weapon();

        //FIRING

        //CONDITIONS

        public void Immobilised(bool immobilised)
        {
            IsImmobilised = immobilised;
        }

        public void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public virtual void Update()
        {
            if (!CombatManager.InCombat()) return;
            if (GetTarget() != null) _distanceToTarget = CurrentCell().Distance(GetTarget().CurrentCell());
            _currentCell = PathingGrid.Instance().PositionToCell(transform.position);
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

        protected bool Moving()
        {
            return _rigidbody.velocity == Vector2.zero;
        }

        public float GetAccuracyModifier()
        {
            float baseRecoilModifier = -0.5f * Recoil.CurrentValue() + 1;
//            float movementModifier = Moving() ? 0.5f : 1f;
            return baseRecoilModifier;
        }

        private void UpdateRecoil()
        {
            if (_recoveryTimer > 0)
            {
                _recoveryTimer -= Time.deltaTime;
                return;
            }

            Recoil.Decrement(RecoilRecoveryRate + Time.deltaTime);
        }

        public void FixedUpdate()
        {
//            KeepInBounds();
            _rigidbody.AddForce(_forceToadd);
            _forceToadd = Vector2.zero;
        }

        private void KeepInBounds()
        {
            Cell current = CurrentCell();
            int x = current.XIndex;
            int y = current.YIndex;
            int distanceToTop = y;
            int distanceToBottom = PathingGrid.GridWidth - 1 - y;
            int distanceToLeft = x;
            int distanceToRight = PathingGrid.GridWidth - 1 - x;
            int threshold = 3;
            float yForceModifier = 1;
            float xForceModifier = 1;

            if (distanceToTop <= threshold && _forceToadd.y < 0) yForceModifier = (float) distanceToTop / threshold - 1;
            else if (distanceToBottom <= threshold && _forceToadd.y > 0)
                yForceModifier = (float) distanceToBottom / threshold - 1;

            if (distanceToLeft <= threshold && _forceToadd.x < 0) xForceModifier = (float) distanceToLeft / threshold - 1;
            else if (distanceToRight <= threshold && _forceToadd.x > 0)
                xForceModifier = (float) distanceToRight / threshold - 1;

            _forceToadd.x *= Mathf.Clamp(xForceModifier, 0f, 1f);
            _forceToadd.y *= Mathf.Clamp(yForceModifier, 0f, 1f);
        }

        protected virtual void Dash(Vector2 direction)
        {
            if (IsImmobilised) return;
            _forceToadd += direction * DashForce;
        }

        public void Move(Vector2 direction)
        {
            if (IsImmobilised) return;
            float speed = Speed;
            if (Sprinting) speed *= SprintModifier;
            _forceToadd += direction * speed;
        }

        protected void StartSprinting()
        {
            if (Sprinting) return;
            Sprinting = true;
        }

        protected void StopSprinting()
        {
            if (!Sprinting) return;
            Sprinting = false;
        }

        public abstract CharacterCombat GetTarget();
    }
}