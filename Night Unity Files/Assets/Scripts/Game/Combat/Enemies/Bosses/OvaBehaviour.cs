using Game.Combat.Generation;
using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses
{
    public class OvaBehaviour : Boss, ITakeDamageInterface
    {
        private HealthController _healthController;

        public override void Awake()
        {
            base.Awake();
            _healthController = new HealthController();
            _healthController.SetInitialHealth(1000, this);
            CombatManager.Enemies().Add(this);
        }
        
        public static void Create()
        {
            GameObject ovaPrefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Ova/Ova");
            Instantiate(ovaPrefab).transform.position = new Vector2(0, 0);
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public void TakeShotDamage(Shot shot)
        {
            _healthController.TakeDamage(shot.DamageDealt());
        }

        public void TakeRawDamage(float damage, Vector2 direction)
        {
            _healthController.TakeDamage(damage);
        }

        public void TakeExplosionDamage(float damage, Vector2 origin)
        {
            _healthController.TakeDamage(damage);
        }

        public void Decay()
        {
        }

        public void Burn()
        {
        }

        public void Sicken(int stacks = 0)
        {
        }

        public bool IsDead()
        {
            return _healthController.GetCurrentHealth() == 0;
        }

        public void Kill()
        {
            KillBoss();
        }
    }
}