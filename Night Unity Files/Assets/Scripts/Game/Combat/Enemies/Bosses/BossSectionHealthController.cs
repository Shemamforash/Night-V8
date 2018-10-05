using Game.Combat.Generation;
using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses
{
    public abstract class BossSectionHealthController : CanTakeDamage
    {
        private int InitialHealth;
        private Boss _boss;

        protected override void Awake()
        {
            base.Awake();
            HealthController.SetInitialHealth(GetInitialHealth(), this);
            CombatManager.Enemies().Add(this);
        }

        protected abstract int GetInitialHealth();

        protected void SetBoss(Boss boss)
        {
            _boss = boss;
        }

        public virtual void Start()
        {
            _boss.RegisterSection(this);
        }

        public bool IsDead()
        {
            return HealthController.GetCurrentHealth() == 0;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public override void Kill()
        {
            base.Kill();
            _boss.UnregisterSection(this);
        }
    }
}