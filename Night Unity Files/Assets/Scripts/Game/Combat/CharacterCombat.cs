using Facilitating.UIControllers;
using Game.Characters;
using Game.Combat.Skills;
using Game.Gear.Weapons;
using SamsHelper;
using SamsHelper.Input;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat
{
    public abstract class CharacterCombat : MonoBehaviour
    {
        public Bleed Bleeding;
        public Burn Burn;
        public Sickness Sick;
        
        private Rigidbody2D _rigidbody;
        private bool Sprinting;
        private const int SprintModifier = 2;
        private bool _immobilised = false;
        private const float DashForce = 300;

//        public bool IsKnockedDown;
//        public bool IsDead;

        public readonly RecoilManager RecoilManager = new RecoilManager();
        private float _distanceToTarget = -1;
        private Cell _currentCell;

        public float Speed;
        private Character _character;

        public float DistanceToTarget()
        {
            if (_distanceToTarget == -1) _distanceToTarget = Vector2.Distance(transform.position, GetTarget().transform.position);
            return _distanceToTarget;
        }

        public Cell CurrentCell()
        {
            return _currentCell == null ? PathingGrid.PositionToCell(transform.position) : _currentCell;
        }

        protected void SetOwnedByEnemy(float speed)
        {
            Speed = speed;
        }

        public virtual void TakeDamage(Shot shot)
        {
            float armourModifier = shot.DidPierce() ? 1 : 1 - ArmourController().CurrentArmour() / 10f;
            float healthDamage = (int) (armourModifier * shot.DamageDealt());
            float armourDamage = (int)((1f - armourModifier) * shot.DamageDealt());
            if (healthDamage != 0) HealthController().TakeDamage(healthDamage);
            if (armourDamage != 0) ArmourController().TakeDamage(armourDamage);
        }

//        private void KnockBack(float distance)
//        {
//            MoveBackwardAction?.Invoke(distance);
//        }

        public virtual void Kill()
        {
//            Destroy(CharacterController.gameObject);
        }


        protected virtual void KnockDown()
        {
//            Interrupt();
//            IsKnockedDown = true;
        }

        public void Knockback(float knockbackDistance)
        {
//            KnockBack(knockbackDistance);
//            KnockDown();
        }

        public virtual bool Immobilised()
        {
//            return IsKnockedDown;
            return false;
        }

        public abstract Weapon Weapon();

        //FIRING

        //CONDITIONS

        protected void SetConditions()
        {
            Bleeding = new Bleed(this);
            Burn = new Burn(this);
            Sick = new Sickness(this);
            Burn.OnConditionNonEmpty = HealthController().StartBurning;
            Burn.OnConditionEmpty = HealthController().StopBurning;
            Bleeding.OnConditionNonEmpty = HealthController().StartBleeding;
            Bleeding.OnConditionEmpty = HealthController().StopBleeding;
        }

        public virtual void ExitCombat()
        {
            Burn.Clear();
            Bleeding.Clear();
            Sick.Clear();
        }

        public void Immobilised(bool immobilised)
        {
            _immobilised = immobilised;
        }

        public void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public virtual void Update()
        {
            if(GetTarget() != null) _distanceToTarget = CurrentCell().Distance(GetTarget().CurrentCell());
            _currentCell = PathingGrid.PositionToCell(transform.position);
            Burn.Update();
            Sick.Update();
            Bleeding.Update();
            RecoilManager.UpdateCombat();
        }

        private Vector2 _forceToadd = Vector2.zero;

        public void FixedUpdate()
        {
            _rigidbody.AddForce(_forceToadd);
            _forceToadd = Vector2.zero;
        }

        protected virtual void Dash(Vector2 direction)
        {
            if (_immobilised) return;
            _forceToadd += direction * DashForce;
        }

        public void Move(Vector2 direction)
        {
            if (_immobilised) return;
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
        public abstract UIArmourController ArmourController();
        public abstract UIHealthBarController HealthController();
    }
}