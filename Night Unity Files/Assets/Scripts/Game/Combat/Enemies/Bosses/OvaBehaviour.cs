using Game.Combat.Generation;
using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses
{
    public class OvaBehaviour : Boss, ITakeDamageInterface
    {
        private HealthController _healthController;
        private const float SpermSpawnThreshold = 200f;
        private float _damageTaken;
        private float SpawnTimer = 15f;
        private DamageSpriteFlash _spriteFlash;

        public override void Awake()
        {
            base.Awake();
            _spriteFlash = GetComponent<DamageSpriteFlash>();
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

        private void TakeDamage(float damage)
        {
            _spriteFlash.FlashSprite();
            _healthController.TakeDamage(damage);
            TrySpawnSperm((int) damage);
        }

        public void TakeShotDamage(Shot shot)
        {
            TakeDamage(shot.DamageDealt());
        }

        public void TakeRawDamage(float damage, Vector2 direction)
        {
            TakeDamage(damage);
        }

        public void TakeExplosionDamage(float damage, Vector2 origin)
        {
            TakeDamage(damage);
        }

        private void TrySpawnSperm(int damage)
        {
            _damageTaken += damage;
            if (!(_damageTaken > SpermSpawnThreshold)) return;
            _damageTaken -= SpermSpawnThreshold;
            SpermBehaviour.Create();
        }

        public void Update()
        {
            if (!CombatManager.IsCombatActive()) return;
            MyUpdate();
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

        public void MyUpdate()
        {
            if (SpawnTimer > 0f)
            {
                SpawnTimer -= Time.deltaTime;
                return;
            }

            SpawnTimer = Random.Range(20f, 30f);
            float normalisedHealth = _healthController.GetNormalisedHealthValue();
            if (normalisedHealth < 0.25f)
            {
                CombatManager.SpawnEnemy(EnemyType.Revenant, Vector2.zero);
            }
            else if (normalisedHealth < 0.5f)
            {
                for (int i = 0; i < Random.Range(1, 4); ++i) CombatManager.SpawnEnemy(EnemyType.Shadow, Vector2.zero);
            }

            for (int i = 0; i < Random.Range(5, 11); ++i) CombatManager.SpawnEnemy(EnemyType.Ghoul, Vector2.zero);
        }
    }
}