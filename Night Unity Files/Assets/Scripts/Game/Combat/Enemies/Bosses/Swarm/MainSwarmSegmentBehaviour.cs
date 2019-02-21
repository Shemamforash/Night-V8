using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.Libraries;
using Sirenix.Utilities;
using UnityEngine;

public class MainSwarmSegmentBehaviour : CanTakeDamage
{
    private float _fireCounter;
    private static float _fireTimer = 2f;
    private float _contractTimer;
    private bool _contracting;
    private float _burstForce;
    private static GameObject[] _swarmPrefabs;
    private float _burstTimer = 10f;
    private Orbit _orbit;
    private float _spawnTimer;
    private float _childSpawnTimer;
    private float _modifiedChildSpawnTimer = 1;
    private const float BaseChildSpawnTimer = 2f;
    private const float ChildSpawnDecayRate = 0.99f;
    private const int InitialSwarmCount = 20;
    private const float FireTimerDecay = 0.95f;

    protected override void Awake()
    {
        base.Awake();
        _orbit = gameObject.AddComponent<Orbit>();
        _fireCounter = Random.Range(_fireTimer, _fireTimer + 2);
        _contractTimer = Random.Range(8f, 12f);
        int initialHealth = WorldState.ScaleValue(1000);
        HealthController.SetInitialHealth(initialHealth, this);
        ArmourController.AutoGenerateArmour();
        SpriteFlash = gameObject.FindChildWithName<DamageSpriteFlash>("Sprite");
    }

    public override string GetDisplayName() => "The Soul of Rha";

    private void Start()
    {
        for (int i = 0; i < InitialSwarmCount; ++i) SpawnChild();
        _orbit.Initialise(PlayerCombat.Instance.transform, GetComponent<Rigidbody2D>().AddForce, 0.25f, 1.5f, 2.5f);
    }

    public override void Kill()
    {
        if (SwarmBehaviour.Instance().CheckChangeToStageTwo())
        {
            HealthController.Heal(100000);
            return;
        }

        base.Kill();
        SwarmBehaviour.Instance().Kill();
    }

    public void Update()
    {
        UpdateFireCounter();
        UpdateContractTimer();
        UpdateBurstCounter();
        UpdateSpawnChildTimer();
        if (!_contracting) _burstForce = Mathf.PerlinNoise(Time.timeSinceLevelLoad, 0f);
        for (int i = SwarmSegmentBehaviour.Active.Count - 1; i >= 0; --i)
            SwarmSegmentBehaviour.Active[i].UpdateSection(_burstForce * 1.3f - 0.3f);
    }

    private void UpdateSpawnChildTimer()
    {
        if (SwarmSegmentBehaviour.Active.Count > 100) return;
        _childSpawnTimer -= Time.deltaTime;
        if (_childSpawnTimer > 0f) return;
        _childSpawnTimer = BaseChildSpawnTimer * _modifiedChildSpawnTimer;
        _modifiedChildSpawnTimer *= ChildSpawnDecayRate;
        if (_modifiedChildSpawnTimer < 1f) _modifiedChildSpawnTimer = 1f;
        SpawnChild();
    }

    private void SpawnChild()
    {
        if (_swarmPrefabs == null) _swarmPrefabs = Resources.LoadAll<GameObject>("Prefabs/Combat/Bosses/Swarm/Segments");
        SwarmSegmentBehaviour swarm = Instantiate(_swarmPrefabs.RandomElement()).GetComponent<SwarmSegmentBehaviour>();
        swarm.SetSwarmParent(SwarmBehaviour.Instance());
    }

    private void UpdateFireCounter()
    {
        if (_fireCounter < 0f) return;
        _fireCounter -= Time.deltaTime;
        if (_fireCounter > 0f) return;
        _fireTimer *= FireTimerDecay;
        if (_fireTimer < 1f) _fireTimer = 1f;
        _fireCounter = Random.Range(_fireTimer, _fireTimer * 2);
        if (SwarmSegmentBehaviour.Active.Count == 0) return;
        SwarmSegmentBehaviour swarmSegment = SwarmSegmentBehaviour.Active[0];
        swarmSegment.StartSeeking();
    }

    private static int SectionCount() => SwarmBehaviour.Instance().Sections.Count;

    private void UpdateBurstCounter()
    {
        if (_burstTimer < 0f) return;
        _burstTimer -= Time.deltaTime;
        if (_burstTimer > 0f) return;
        SwarmSegmentBehaviour.Active.ForEach(s => s.StartBurst());
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(4f);
        sequence.AppendCallback(() => _burstTimer = 10);
    }

    private void UpdateContractTimer()
    {
        if (_contractTimer < 0f) return;
        _contractTimer -= Time.deltaTime;
        if (_contractTimer > 0f) return;
        _contractTimer = Random.Range(8f, 12f);
        _contracting = true;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(() => _burstForce, f => _burstForce = f, -1f, 1f).SetEase(Ease.OutBounce));
        sequence.AppendCallback(() =>
        {
            float angleInterval = 360 / 3f;
            for (float angle = 0f; angle < 360f; angle += angleInterval)
            {
                Vector3 direction = AdvancedMaths.CalculatePointOnCircle(angle, 1, Vector2.zero);
                MaelstromShotBehaviour.Create(direction, transform.position + direction * 0.5f, 3f, false);
            }

            _contracting = false;
        });
    }

    private void UpdateSpawnTimer()
    {
        if (SwarmSegmentBehaviour.Active.Count > 100) return;

        List<CanTakeDamage> enemies = CombatManager.Enemies();
        enemies.RemoveAll(e => e is SwarmSegmentBehaviour);
        if (enemies.Count > 10) return;
        if (_spawnTimer < 0f)
        {
            float spawnTime = 12 - SwarmSegmentBehaviour.Active.Count / 10f;
            _spawnTimer = Random.Range(spawnTime * 0.75f, spawnTime * 1.25f);
            switch (Random.Range(0, 4))
            {
                case 0:
                    CombatManager.SpawnEnemy(EnemyType.Shadow, transform.position);
                    break;
                case 1:
                    CombatManager.SpawnEnemy(EnemyType.Ghast, transform.position);
                    break;
                case 2:
                    CombatManager.SpawnEnemy(EnemyType.Ghoul, transform.position);
                    break;
                case 3:
                    CombatManager.SpawnEnemy(EnemyType.Revenant, transform.position);
                    break;
            }
        }

        _spawnTimer -= Time.deltaTime;
    }
}