using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Gear.Armour;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses
{
    public class OvaBehaviour : CanTakeDamage
    {
        private const float SpermSpawnThreshold = 250f;
        private float _damageTakenSinceLastSpawn;
        private float SpawnTimer;
        private Boss _boss;
        private float _beamTimer;
        private float _pushTimer;
        private int _startingHealth = 1000;

        protected override void Awake()
        {
            base.Awake();
            _boss = GetComponent<Boss>();
            _startingHealth = WorldState.ScaleValue(1000);
            HealthController.SetInitialHealth(_startingHealth, this);
            ArmourController.AutoGenerateArmour();
            TryAddBeamAttack();
            SpermBehaviour.Create();
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
            CheckAddBurst(healthBefore, healthAfter);
            TrySpawnSperm(damageTaken);
            if (HealthController.GetCurrentHealth() != 0) return;
            _boss.Kill();
        }

        private void CheckAddExplosion(int healthBefore, int healthAfter)
        {
            float threshold = _startingHealth * 0.4f;
            if (healthBefore <= threshold || healthAfter > threshold) return;
            gameObject.AddComponent<OvaExplosionAttack>().Initialise(2f, 7f);
        }

        private void CheckAddBurst(int healthBefore, int healthAfter)
        {
            float threshold = _startingHealth * 0.75f;
            if (healthBefore <= threshold || healthAfter > threshold) return;
            gameObject.AddComponent<OvaBurstAttack>().Initialise(3f, 6f);
        }

        private void TrySpawnSperm(int damage)
        {
            _damageTakenSinceLastSpawn += damage;
            if (!(_damageTakenSinceLastSpawn > SpermSpawnThreshold)) return;
            _damageTakenSinceLastSpawn -= SpermSpawnThreshold;
            SpermBehaviour.Create();
            ArmourController.Repair(WorldState.ScaleValue(5000));
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            TrySpawnEnemy();
            TryPushBackPlayer();
        }

        private void TryPushBackPlayer()
        {
            if (_pushTimer > 0f)
            {
                _pushTimer -= Time.deltaTime;
                return;
            }

            if (PlayerCombat.Instance.transform.Distance(transform) > 1.5f) return;
            float rotation = AdvancedMaths.AngleFromUp(transform.position, PlayerCombat.Position());
            PushController.Create(transform.position, rotation, false, 360);
            _pushTimer = 0.5f;
        }

        private void TrySpawnEnemy()
        {
            SpawnTimer -= Time.deltaTime;
            if (SpawnTimer > 0f) return;
            SpawnTimer = Random.Range(20f, 30f);
            float normalisedHealth = HealthController.GetNormalisedHealthValue();
            if (normalisedHealth < 0.25f)
                CombatManager.Instance().SpawnEnemy(EnemyType.Revenant, Vector2.zero);
            else if (normalisedHealth < 0.5f)
                for (int i = 0; i < Random.Range(1, 4); ++i)
                    CombatManager.Instance().SpawnEnemy(EnemyType.Shadow, Vector2.zero);
            for (int i = 0; i < Random.Range(5, 11); ++i) CombatManager.Instance().SpawnEnemy(EnemyType.Ghoul, Vector2.zero);
        }

        private void TryAddBeamAttack()
        {
            Transform beam = gameObject.FindChildWithName("Beams").transform;
            for (int i = 0; i < beam.childCount; ++i)
                beam.GetChild(i).gameObject.AddComponent<Beam>().Initialise(10f, 5f);
        }

        public override string GetDisplayName()
        {
            return "Ahna's Despair";
        }
    }
}