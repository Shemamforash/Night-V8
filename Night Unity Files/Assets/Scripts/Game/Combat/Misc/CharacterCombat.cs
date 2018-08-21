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

        private float _recoveryTimer;

        public ArmourController ArmourController;
        public HealthController HealthController = new HealthController();
        private CharacterCombat _target;
        public MovementController MovementController;
        protected CharacterUi CharacterUi;
        private BloodSpatter _bloodSpatter;

        private bool _isDead;
        private const int ConditionTicksMax = 5;
        private const float SicknessDurationMax = 5f;
        private const int SicknessTargetTicks = 10;
        private int _burnTicks, _decayTicks;
        protected int SicknessStacks;
        private float _burnDuration, _decayDuration, _sicknessDuration;

        public Shield Shield;

        public GameObject GetGameObject()
        {
            return gameObject;
        }
        
        public float DistanceToTarget()
        {
            if (_distanceToTarget == -1) _distanceToTarget = Vector2.Distance(transform.position, GetTarget().transform.position);
            return _distanceToTarget;
        }

        public Vector3 Direction() => transform.up;

        public void Burn()
        {
            if (_burnTicks == 0)
            {
                CharacterUi.GetHealthController(this).StartBurning();
                if (this is EnemyBehaviour) PlayerCombat.Instance.Player.BrandManager.IncreaseBurnCount();
            }

            _burnDuration = 1;
            _burnTicks = ConditionTicksMax;
        }

        public bool IsBurning() => _burnTicks > 0;

        public void Decay()
        {
            if (_decayTicks == 0)
            {
                CharacterUi.GetHealthController(this).StartDecay();
                if (this is EnemyBehaviour) PlayerCombat.Instance.Player.BrandManager.IncreaseDecayCount();
            }

            _decayDuration = 1;
            _decayTicks = ConditionTicksMax;
        }

        public bool IsDecaying() => _decayTicks > 0;

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
            _decayTicks = 0;
            _burnTicks = 0;
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
            int decayDamage = Mathf.FloorToInt(ArmourPlate.PlateHealthUnit * 0.05f);
            if (decayDamage < 1) decayDamage = 1;
            return decayDamage;
        }

        protected virtual int GetSicknessTargetTicks()
        {
            return SicknessTargetTicks;
        }

        private void UpdateConditions()
        {
            if (_burnTicks > 0)
            {
                if (_burnDuration <= 0)
                {
                    _burnDuration = 1 - _burnDuration;
                    HealthController.TakeDamage(GetBurnDamage());
                    --_burnTicks;
                }
                else
                {
                    _burnDuration -= Time.deltaTime;
                }
            }
            else
            {
                CharacterUi.GetHealthController(this)?.StopBurning();
            }

            if (_decayTicks > 0)
            {
                if (_burnDuration <= 0)
                {
                    _decayDuration = 1 - _decayDuration;
                    TakeArmourDamage(GetDecayDamage());
                    --_decayTicks;
                }
                else
                {
                    _decayDuration -= Time.deltaTime;
                }
            }
            else
            {
                CharacterUi.GetHealthController(this)?.StopDecaying();
            }

            CharacterUi.GetHealthController(this)?.SetSicknessLevel((float) SicknessStacks / GetSicknessTargetTicks());
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

        public void TakeArmourDamage(float damage)
        {
            ArmourController.TakeDamage(this, Mathf.CeilToInt(damage));
        }

        public Cell CurrentCell() => PathingGrid.WorldToCellPosition(transform.position);

        public virtual void TakeShotDamage(Shot shot)
        {
            MovementController.Knockback(shot.transform.position, shot._knockBackForce);
            float armourProtection = ArmourController.GetCurrentArmour() / 10f;
            float armourDamage = shot.DamageDealt() * armourProtection;
            float healthDamage = shot.DamageDealt() - armourDamage;
            TakeArmourDamage(armourDamage);
            HealthController.TakeDamage(Mathf.CeilToInt(healthDamage));
            if (_bloodSpatter == null) return;
            _bloodSpatter.Spray(shot.Direction(), healthDamage);
        }

        public bool IsDead()
        {
            return _isDead;
        }

        public void TakeRawDamage(float damage, Vector2 direction)
        {
            float armourProtection = ArmourController.GetCurrentArmour() / 10f;
            float armourDamage = damage * armourProtection;
            float healthDamage = damage - armourDamage;
            TakeArmourDamage(armourDamage);
            HealthController.TakeDamage(Mathf.CeilToInt(healthDamage));
            if (_bloodSpatter == null) return;
            _bloodSpatter.Spray(direction, healthDamage);
        }

        public void TakeExplosionDamage(float damage, Vector2 origin)
        {
            HealthController.TakeDamage(damage);
            Vector2 dir = (Vector2) transform.position - origin;
            float distance = dir.magnitude;
            dir.Normalize();
            MovementController.AddForce(dir * 1f / distance * 10f);
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
            MovementController = GetComponent<MovementController>();
            _bloodSpatter = GetComponent<BloodSpatter>();
            if (this is EnemyBehaviour) CharacterUi = EnemyUi.Instance();
            else CharacterUi = PlayerUi.Instance();
            WeaponAudio = gameObject.FindChildWithName<WeaponAudioController>("Weapon Audio");
            Shield = gameObject.FindChildWithName<Shield>("Shield");
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