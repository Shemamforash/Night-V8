using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Gear.Armour;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses
{
    public class OvaBehaviour : CanTakeDamage
    {
        private const float SpermSpawnThreshold = 200f;
        private float _damageTakenSinceLastSpawn;
        private float SpawnTimer = 15f;
        private Boss _boss;
        private const int StartingHealth = 2000;

        protected override void Awake()
        {
            base.Awake();
            _boss = GetComponent<Boss>();
            HealthController.SetInitialHealth(StartingHealth, this);
            ArmourController = new ArmourController(null);
            ArmourController.AutoFillSlots(10);
        }

        public static void Create()
        {
            GameObject ovaPrefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Ova/Ova");
            Instantiate(ovaPrefab).transform.position = new Vector2(0, 0);
        }

        protected override void TakeDamage(int damage, Vector2 direction)
        {
            int healthBefore = (int) HealthController.GetCurrentHealth();
            base.TakeDamage(damage, direction);
            int healthAfter = (int) HealthController.GetCurrentHealth();
            int damageTaken = healthBefore - healthAfter;
            CheckAddExplosion(healthBefore, healthAfter);
            TrySpawnSperm(damageTaken);
            if (HealthController.GetCurrentHealth() != 0) return;
            _boss.Kill();
        }

        private void CheckAddExplosion(int healthBefore, int healthAfter)
        {
            if (healthBefore <= StartingHealth / 2 || healthAfter > StartingHealth / 2f) return;
            gameObject.AddComponent<OvaExplosionAttack>().Initialise(15f, 10f);
        }

        private void TrySpawnSperm(int damage)
        {
            _damageTakenSinceLastSpawn += damage;
            if (!(_damageTakenSinceLastSpawn > SpermSpawnThreshold)) return;
            _damageTakenSinceLastSpawn -= SpermSpawnThreshold;
            SpermBehaviour.Create();
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            if (SpawnTimer > 0f)
            {
                SpawnTimer -= Time.deltaTime;
                return;
            }

            SpawnTimer = Random.Range(20f, 30f);

            float normalisedHealth = HealthController.GetNormalisedHealthValue();
            if (normalisedHealth < 0.25f)
                CombatManager.SpawnEnemy(EnemyType.Revenant, Vector2.zero);
            else if (normalisedHealth < 0.5f)
                for (int i = 0; i < Random.Range(1, 4); ++i)
                    CombatManager.SpawnEnemy(EnemyType.Shadow, Vector2.zero);
            for (int i = 0; i < Random.Range(5, 11); ++i) CombatManager.SpawnEnemy(EnemyType.Ghoul, Vector2.zero);
        }

        public override string GetDisplayName()
        {
            return "Ahna, The Dead One";
        }
    }
}