using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class SwarmBehaviour : Boss
{
    private static readonly List<SwarmBehaviour> _swarms = new List<SwarmBehaviour>();
    private const int SwarmCount = 150;
    private float _fireCounter;
    private float _fireCounterMin, _fireCounterMax;
    private float _contractTimer;
    private bool _contracting;
    private float _burstForce;
    private static GameObject[] _swarmPrefabs;
    private float _burstCounter;
    private Orbit _orbit;
    private float _spawnTimer;

    public static void Create()
    {
        for (int i = 0; i < 360; i += 120)
        {
            Vector2 position = AdvancedMaths.CalculatePointOnCircle(i, 4f, Vector2.zero);
            CreateNew().Initialise(position);
        }
    }

    private static SwarmBehaviour CreateNew()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Swarm/Swarm Boss");
        GameObject swarm = Instantiate(prefab);
        return swarm.GetComponent<SwarmBehaviour>();
    }

    private void Initialise(Vector2 position)
    {
        transform.position = position;
        SpawnNewChildren();
        RecalculateFireTimer();
    }

    private void SpawnNewChildren()
    {
        if (_swarmPrefabs == null) _swarmPrefabs = Resources.LoadAll<GameObject>("Prefabs/Combat/Bosses/Swarm/Segments");
        for (int i = 0; i < SwarmCount; ++i)
        {
            SwarmSegmentBehaviour swarm = Instantiate(_swarmPrefabs.RandomElement()).GetComponent<SwarmSegmentBehaviour>();
            swarm.SetSwarmParent(this);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _orbit = gameObject.AddComponent<Orbit>();

        if (Random.Range(0, 2) == 0) _orbit.SwitchSpin();
        _swarms.Add(this);
        _fireCounter = Random.Range(_fireCounterMin, _fireCounterMax);
        _contractTimer = Random.Range(8f, 12f);
    }

    private void Start()
    {
        float minOrbitRadius = Random.Range(1.5f, 2.5f);
        float maxOrbitRadius = minOrbitRadius * 2f;
        float moveSpeed = Random.Range(1f, 2f);
        _orbit.Initialise(PlayerCombat.Instance.transform, RigidBody.AddForce, moveSpeed, minOrbitRadius, maxOrbitRadius);
    }

    private void RecalculateFireTimer()
    {
        _fireCounterMax = 1f / 200f * SectionCount() + 0.5f;
        _fireCounterMin = 1f / 300f * SectionCount() + 0.25f;
    }

    private void UpdateFireCounter()
    {
        if (_burstCounter < 0f) return;
        _fireCounter -= Time.deltaTime;
        if (_fireCounter > 0f) return;
        _fireCounter = Random.Range(_fireCounterMin, _fireCounterMax);
        if (SectionCount() == 0) return;
        SwarmSegmentBehaviour swarmSegment = (SwarmSegmentBehaviour) Sections[0];
        swarmSegment.StartSeeking();
    }

    private void UpdateBurstCounter()
    {
        if (SectionCount() > 100) return;
        if (_burstCounter < 0f) return;
        _burstCounter -= Time.deltaTime;
        if (_burstCounter > 0f) return;
        Sections.ForEach(s => ((SwarmSegmentBehaviour) s).StartBurst());
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(4f);
        sequence.AppendCallback(() => _burstCounter = Random.Range(10, 20));
    }

    private void UpdateContractTimer()
    {
        if (_burstCounter < 0f) return;
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
                MaelstromShotBehaviour.Create(direction, transform.position + direction * 0.5f, 3f);
            }

            _contracting = false;
        });
    }

    private void UpdateSpawnTimer()
    {
        if (Sections.Count > 100) return;

        if (_spawnTimer < 0f)
        {
            float spawnTime = 12 - Sections.Count / 10f;
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

    public void Update()
    {
        UpdateFireCounter();
        UpdateContractTimer();
        UpdateBurstCounter();
        UpdateSpawnTimer();
        if (!_contracting) _burstForce = Mathf.PerlinNoise(Time.timeSinceLevelLoad, 0f);
        for (int i = Sections.Count - 1; i >= 0; --i)
            ((SwarmSegmentBehaviour) Sections[i]).UpdateSection(transform.position, _burstForce * 1.3f - 0.3f);
    }

    public override void UnregisterSection(BossSectionHealthController segment)
    {
        Sections.Remove(segment);
        RecalculateFireTimer();
        if (Sections.Count != 0) return;
        _swarms.Remove(this);
        if (_swarms.Count == 0) Kill();
        Destroy(gameObject);
    }

    public static List<CanTakeDamage> GetAllSegments()
    {
        List<CanTakeDamage> segments = new List<CanTakeDamage>();
        _swarms.ForEach(s => { segments.AddRange(s.Sections); });
        return segments;
    }
}