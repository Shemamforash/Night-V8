using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Gear.Armour;
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
        private const int StartingHealth = 2000;

        protected override void Awake()
        {
            base.Awake();
            _boss = GetComponent<Boss>();
            HealthController.SetInitialHealth(StartingHealth, this);
            ArmourController = new ArmourController(null);
            ArmourController.AutoFillSlots(10);
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
            TrySpawnSperm(damageTaken);
            if (HealthController.GetCurrentHealth() != 0) return;
            _boss.Kill();
        }

        private void CheckAddExplosion(int healthBefore, int healthAfter)
        {
            if (healthBefore <= StartingHealth / 2 || healthAfter > StartingHealth / 2f) return;
            gameObject.AddComponent<OvaExplosionAttack>().Initialise(2f, 4f);
        }

        private void TrySpawnSperm(int damage)
        {
            _damageTakenSinceLastSpawn += damage;
            if (!(_damageTakenSinceLastSpawn > SpermSpawnThreshold)) return;
            _damageTakenSinceLastSpawn -= SpermSpawnThreshold;
            SpermBehaviour.Create();
            ArmourController.Repair(600);
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            RepairArmour();
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
            float rotation = AdvancedMaths.AngleFromUp(transform.position, PlayerCombat.Instance.transform.position);
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
                CombatManager.SpawnEnemy(EnemyType.Revenant, Vector2.zero);
            else if (normalisedHealth < 0.5f)
                for (int i = 0; i < Random.Range(1, 4); ++i)
                    CombatManager.SpawnEnemy(EnemyType.Shadow, Vector2.zero);
            for (int i = 0; i < Random.Range(5, 11); ++i) CombatManager.SpawnEnemy(EnemyType.Ghoul, Vector2.zero);
        }

        private void TryAddBeamAttack()
        {
            Transform beam = gameObject.FindChildWithName("Beams").transform;
            for (int i = 0; i < beam.childCount; ++i)
                beam.GetChild(i).gameObject.AddComponent<Beam>().Initialise(5f, 5f);
        }

        private void RepairArmour()
        {
            ArmourController.Repair(5 * Time.deltaTime);
        }

        public override string GetDisplayName()
        {
            return "Ahna, The Dead One";
        }
    }
}