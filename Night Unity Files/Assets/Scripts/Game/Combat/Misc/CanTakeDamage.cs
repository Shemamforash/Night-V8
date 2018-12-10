using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Gear.Armour;
using Game.Global;
using UnityEngine;

namespace Game.Combat.Misc
{
    public abstract class CanTakeDamage : MonoBehaviour
    {
        private DamageSpriteFlash _spriteFlash;
        private BloodSpatter _bloodSpatter;
        public ArmourController ArmourController;
        public HealthController HealthController = new HealthController();
        private const float SicknessDurationMax = 5f;
        private const int SicknessTargetTicks = 10;
        private float _timeSinceLastBurn;
        protected int SicknessStacks;
        private float _sicknessDuration;
        private float _markTime;
        private bool _marked;
        public bool IsPlayer;

        public void TakeArmourDamage(float damage)
        {
            ArmourController?.TakeDamage(Mathf.CeilToInt(damage));
        }

        protected virtual void Awake()
        {
            _spriteFlash = GetComponent<DamageSpriteFlash>();
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
            UpdateMarkTime();
        }

        public abstract string GetDisplayName();

        public virtual void Burn()
        {
            if (_timeSinceLastBurn < 1f) return;
            _timeSinceLastBurn = 0f;
            _spriteFlash.FlashSprite();
            HealthController.TakeDamage(GetBurnDamage());
        }

        public virtual void Decay()
        {
            TakeArmourDamage(GetDecayDamage());
        }

        public virtual void Sicken(int stacks = 1)
        {
            SicknessStacks += stacks;
            if (SicknessStacks >= GetSicknessTargetTicks())
            {
                _spriteFlash.FlashSprite();
                float damage = (WorldState.Difficulty() / 25f + 1f) * 50f;
                HealthController.TakeDamage(damage);
                SicknessStacks = 0;
            }

            _sicknessDuration = 0;
        }

        public bool IsSick() => SicknessStacks > 0;

        public void ClearConditions()
        {
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
            _spriteFlash.FlashSprite();
            float armourModifier = 1;
            if (ArmourController != null) armourModifier = ArmourController.CalculateDamageModifier();
            int healthDamage = Mathf.CeilToInt(damage * armourModifier);
            HealthController.TakeDamage(healthDamage);
            if (_bloodSpatter != null) _bloodSpatter.Spray(direction, healthDamage);
            if (HealthController.GetCurrentHealth() != 0) return;
            LeafBehaviour.CreateLeaves(direction, transform.position);
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

        public void Mark()
        {
            _marked = true;
            _markTime = 5f;
        }

        private void UpdateMarkTime()
        {
            if (!_marked) return;
            if (_markTime < 0f) return;
            _markTime -= Time.deltaTime;
            if (_markTime < 0f) PlayerCombat.Instance.EndMark(this);
        }
    }
}