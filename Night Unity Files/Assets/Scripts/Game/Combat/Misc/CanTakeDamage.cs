using System.Collections.Generic;
using Game.Characters;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Gear.Armour;
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
        private readonly List<float> _burnTicks = new List<float>();
        private float _timeSinceLastBurn;
        private const float BurnTickDuration = 4f;
        protected int SicknessStacks;
        private float _sicknessDuration;
        private float _markTime;
        private bool _marked;

        public void TakeArmourDamage(float damage)
        {
            ArmourController?.TakeDamage(Mathf.CeilToInt(damage));
        }

        protected virtual void Awake()
        {
            _spriteFlash = GetComponent<DamageSpriteFlash>();
            _bloodSpatter = GetComponent<BloodSpatter>();
            if (this is PlayerCombat) return;
            CombatManager.Instance().AddEnemy(this);
        }

        public virtual void Kill()
        {
            Destroy(gameObject);
            if (this is PlayerCombat) return;
            CombatManager.Remove(this);
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
            _burnTicks.Add(BurnTickDuration);
        }

        protected int GetBurnTicks()
        {
            return _burnTicks.Count;
        }

        public bool IsBurning() => _burnTicks.Count > 0;

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
                HealthController.TakeDamage(HealthController.GetMaxHealth() / 4f);
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

        public float GetSicknessLevel()
        {
            return (float) SicknessStacks / GetSicknessTargetTicks();
        }

        private void UpdateBurn()
        {
            _timeSinceLastBurn += Time.deltaTime;
            if (_burnTicks.Count == 0) return;
            for (int i = _burnTicks.Count - 1; i >= 0; --i)
            {
                float burnTick = _burnTicks[i];
                float newBurnTick = burnTick - Time.deltaTime;
                if (burnTick >= 3f && newBurnTick < 3f || burnTick >= 2f && newBurnTick < 2f || burnTick >= 1f && newBurnTick < 1f || burnTick > 0f && newBurnTick < 0f)
                {
                    _spriteFlash.FlashSprite();
                    HealthController.TakeDamage(GetBurnDamage());
                }

                _burnTicks[i] = newBurnTick;
                if (newBurnTick < 0f)
                {
                    _burnTicks.RemoveAt(i);
                }
            }
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
            int damageDealt = shot.Attributes().DamageDealt();
            TakeDamage(damageDealt, shot.Direction());
            if (this is PlayerCombat) return;
            PlayerCombat.Instance.UpdateAdrenaline(damageDealt);
        }

        protected virtual void TakeDamage(int damage, Vector2 direction)
        {
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
            TakeDamage(damage, direction);
        }

        public virtual void TakeExplosionDamage(int damage, Vector2 origin, float radius)
        {
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