using Game.Combat.Misc;
using Game.Gear.Armour;

namespace Game.Combat.Enemies.Bosses
{
    public abstract class BossSectionHealthController : CanTakeDamage
    {
        private int InitialHealth;
        private Boss _boss;

        protected override void Awake()
        {
            base.Awake();
            ArmourController = new ArmourController(null);
        }

        protected void UpdateInitialHealth()
        {
            HealthController.SetInitialHealth(GetInitialHealth(), this);
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

        public override void Kill()
        {
            base.Kill();
            _boss.UnregisterSection(this);
        }
    }
}