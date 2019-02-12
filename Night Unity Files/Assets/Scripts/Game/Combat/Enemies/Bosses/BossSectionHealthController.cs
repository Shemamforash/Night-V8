using Game.Combat.Misc;

namespace Game.Combat.Enemies.Bosses
{
    public abstract class BossSectionHealthController : CanTakeDamage
    {
        private int InitialHealth;
        protected Boss Parent;

        protected override void Awake()
        {
            base.Awake();
            gameObject.layer = 24;
            UpdateInitialHealth();
        }

        protected void UpdateInitialHealth()
        {
            HealthController.SetInitialHealth(GetInitialHealth(), this);
        }

        protected abstract int GetInitialHealth();

        protected void SetBoss(Boss boss)
        {
            if (Parent != null) Parent.UnregisterSection(this);
            Parent = boss;
            Parent.RegisterSection(this);
        }

        public override void Kill()
        {
            Parent.UnregisterSection(this);
            base.Kill();
        }
    }
}