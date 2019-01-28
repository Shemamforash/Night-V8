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
        private const float SicknessDurationMax = 5f;
        private const int SicknessTargetTicks = 10;
        private float _timeSinceLastBurn;
        protected int SicknessStacks;
        private float _sicknessDuration;
        public bool IsPlayer;

        private void TakeArmourDamage(int damage)
        {
            if (!ArmourController.CanAbsorbDamage()) return;
            ArmourController.TakeDamage(damage);
        }

        protected virtual void Awake()
        {
            SpriteFlash = GetComponent<DamageSpriteFlash>();
            _bloodSpatter = GetComponent<BloodSpatter>();
            if (!IsPlayer) CombatManager.AddEnemy(this);
        }

        public virtual void Kill()
        {
            if (!IsPlayer) CombatManager.RemoveEnemy(this);
            Destroy(gameObject);
        }

        public virtual void MyUpdate()
        {
            UpdateConditions();
            ArmourController.Update();
        }

        public abstract string GetDisplayName();

        public virtual bool Burn()
        {
            if (_timeSinceLastBurn < 0.5f) return false;
            _timeSinceLastBurn = 0f;
            SpriteFlash.FlashSprite();
            HealthController.TakeDamage(GetBurnDamage());
            return true;
        }

        public virtual void Decay()
        {
            TakeArmourDamage(10000);
        }

        public virtual bool Sicken(int stacks = 1)
        {
            bool tookDamage = false;
            SicknessStacks += stacks;
            if (SicknessStacks >= GetSicknessTargetTicks())
            {
                SpriteFlash.FlashSprite();
                float damage = HealthController.GetMaxHealth() / 10f;
                HealthController.TakeDamage(damage);
                SicknessStacks = 0;
                tookDamage = true;
            }

            _sicknessDuration = 0;
            return tookDamage;
        }

        public bool IsSick() => SicknessStacks > 0;

        protected virtual int GetBurnDamage()
        {
            int burnDamage = Mathf.FloorToInt(HealthController.GetMaxHealth() * 0.01f);
            if (burnDamage < 1) burnDamage = 1;
            return burnDamage;
        }

        protected virtual int GetSicknessTargetTicks()
        {
            return SicknessTargetTicks;
        }

        public float GetSicknessLevel()
        {
            return (float) SicknessStacks / GetSicknessTargetTicks();
        }

        private void UpdateBurn()
        {
            _timeSinceLastBurn += Time.deltaTime;
        }

        private void UpdateSickness()
        {
            if (SicknessStacks == 0) return;
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