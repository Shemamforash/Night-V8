using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Gear.Armour;
using Game.Global;
using UnityEngine;

namespace Game.Combat.Misc
{
    public abstract class CanTakeDamage : MonoBehaviour
    {
        protected DamageSpriteFlash SpriteFlash;
        protected BloodSpatter _bloodSpatter;
        public ArmourController ArmourController = new ArmourController();
        public readonly HealthController HealthController = new HealthController();
        private const float VoidDurationMax = 5f;
        private const int VoidTargetTicks = 5;
        private float _timeSinceLastBurn;
        protected int VoidStacks;
        private float _voidDuration;
        public bool IsPlayer;
        protected float BurnDamagePercent = 0.05f;

        private void TakeArmourDamage(int damage)
        {
            if (!ArmourController.CanAbsorbDamage()) return;
            ArmourController.TakeDamage(damage);
        }

        protected virtual void Awake()
        {
            SpriteFlash = GetComponent<DamageSpriteFlash>();
            _bloodSpatter = GetComponent<BloodSpatter>();
            if (!IsPlayer) CombatManager.Instance().AddEnemy(this);
        }

        public virtual void Kill()
        {
            CombatManager.Instance().RemoveEnemy(this);
            Destroy(gameObject);
        }

        public virtual void MyUpdate()
        {
            UpdateConditions();
            ArmourController.Update();
        }

        public abstract string GetDisplayName();

        public virtual int Burn()
        {
            if (_timeSinceLastBurn < 0.5f) return -1;
            _timeSinceLastBurn = 0f;
            SpriteFlash.FlashSprite();
            int burnDamage = Mathf.FloorToInt(HealthController.GetMaxHealth() * BurnDamagePercent);
            if (burnDamage < 1) burnDamage = 1;
            HealthController.TakeDamage(burnDamage);
            return burnDamage;
        }

        public virtual void Decay()
        {
            TakeArmourDamage(10000);
        }

        public virtual bool Void(int stacks = 1)
        {
            bool tookDamage = false;
            VoidStacks += stacks;
            if (VoidStacks >= GetVoidTargetTicks())
            {
                SpriteFlash.FlashSprite();
                float damage = HealthController.GetMaxHealth() / 10f;
                HealthController.TakeDamage(damage);
                VoidStacks = 0;
                tookDamage = true;
            }

            _voidDuration = 0;
            return tookDamage;
        }

        public bool IsVoided() => VoidStacks > 0;


        private int GetVoidTargetTicks() => VoidTargetTicks;

        public float GetVoid() => (float) VoidStacks / GetVoidTargetTicks();

        private void UpdateBurn()
        {
            _timeSinceLastBurn += Time.deltaTime;
        }

        private void UpdateVoid()
        {
            if (VoidStacks == 0) return;
            if (_voidDuration > VoidDurationMax)
            {
                _voidDuration = 0;
                --VoidStacks;
            }
            else
            {
                _voidDuration += Time.deltaTime;
            }
        }

        private void UpdateConditions()
        {
            UpdateBurn();
            UpdateVoid();
        }

        public virtual void TakeShotDamage(Shot shot)
        {
            if (SceneChanger.ChangingScene()) return;
            int damageDealt = shot.Attributes().DamageDealt();
            TakeDamage(damageDealt, shot.Direction());
            if (IsPlayer) return;
            PlayerCombat.Instance.UpdateAdrenaline(damageDealt);
        }

        protected virtual void TakeDamage(int damage, Vector2 direction)
        {
            if (SceneChanger.ChangingScene()) return;
            if (!IsPlayer && MarkController.InMarkArea(transform.position)) damage *= 2;
            SpriteFlash.FlashSprite();
            if (!ArmourController.CanAbsorbDamage())
            {
                HealthController.TakeDamage(damage);
                if (_bloodSpatter != null) _bloodSpatter.Spray(direction, damage);
                if (HealthController.GetCurrentHealth() != 0) return;
                LeafBehaviour.CreateLeaves(direction, transform.position);
            }
            else ArmourController.TakeDamage(damage);
        }

        public virtual void TakeRawDamage(int damage, Vector2 direction)
        {
            if (SceneChanger.ChangingScene()) return;
            TakeDamage(damage, direction);
        }

        public virtual void TakeExplosionDamage(int damage, Vector2 origin, float radius)
        {
            if (SceneChanger.ChangingScene()) return;
            Vector2 direction = (origin - (Vector2) transform.position).normalized;
            TakeDamage(damage, direction);
        }
    }
}