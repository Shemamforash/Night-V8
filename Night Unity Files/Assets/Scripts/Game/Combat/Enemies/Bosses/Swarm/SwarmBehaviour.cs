using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Misc;
using Game.Combat.Player;
using UnityEngine;

public class SwarmBehaviour : Boss
{
    private static readonly List<SwarmBehaviour> _swarms = new List<SwarmBehaviour>();
    private float _moveSpeed;
    private const int SwarmCount = 500;
    private float _fireCounter;
    private float _fireCounterMin, _fireCounterMax;
    private float _contractTimer;
    private bool _contracting;
    private float _burstForce;
    private static GameObject _swarmPrefab;
    private float _burstCounter;
    private Orbit _orbit;
    private int FirstSplitThreshold, SecondSplitThreshold, ThirdSplitThreshold;

    public static void Create()
    {
        CreateNew().Initialise(null, Vector2.zero);
    }

    private static SwarmBehaviour CreateNew()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Swarm/Swarm Boss");
        GameObject swarm = Instantiate(prefab);
        return swarm.GetComponent<SwarmBehaviour>();
    }

    private static void Create(List<SwarmSegmentBehaviour> inheritedChildren, Vector2 position)
    {
        CreateNew().Initialise(inheritedChildren, position);
    }

    private void Initialise(List<SwarmSegmentBehaviour> inheritedChildren, Vector2 position)
    {
        transform.position = position;
        if (inheritedChildren == null) SpawnNewChildren();
        else inheritedChildren.ForEach(s => { s.SetSwarmParent(this); });
        RecalculateFireTimer();
    }

    private void SpawnNewChildren()
    {
        if (_swarmPrefab == null) _swarmPrefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Swarm/Swarm Segment");
        for (int i = 0; i < SwarmCount; ++i)
        {
            SwarmSegmentBehaviour swarm = Instantiate(_swarmPrefab).GetComponent<SwarmSegmentBehaviour>();
            swarm.SetSwarmParent(this);
        }
    }

    private void Split()
    {
        int centreCount = (int) (SectionCount() / 2f);
        if (centreCount == 0) return;
        List<SwarmSegmentBehaviour> transferredSwarmSegments = new List<SwarmSegmentBehaviour>();
        for (int i = centreCount - 1; i >= 0; --i)
        {
            transferredSwarmSegments.Add((SwarmSegmentBehaviour) Sections[i]);
            Sections.RemoveAt(i);
        }

        Create(transferredSwarmSegments, transform.position);
    }

    protected override void Awake()
    {
        base.Awake();
        FirstSplitThreshold = (int) (SwarmCount * 0.75f);
        SecondSplitThreshold = (int) (SwarmCount * 0.5f);
        ThirdSplitThreshold = (int) (SwarmCount * 0.25f);
        _orbit = gameObject.AddComponent<Orbit>();
        _moveSpeed = Random.Range(0.5f, 1f);
        if (Random.Range(0, 2) == 0) _orbit.SwitchSpin();
        _swarms.Add(this);
        _fireCounter = Random.Range(_fireCounterMin, _fireCounterMax);
        _contractTimer = Random.Range(8f, 12f);
    }

    private void Start()
    {
        float minOrbitRadius = Random.Range(1.5f, 2.5f);
        float maxOrbitRadius = minOrbitRadius * 2f;
        _orbit.Initialise(PlayerCombat.Instance.transform, RigidBody.AddForce, _moveSpeed, minOrbitRadius, maxOrbitRadius);
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
                Vector3 direction = new Vector2();
                direction.x = Mathf.Cos(angle);
                direction.y = Mathf.Sin(angle);
                MaelstromShotBehaviour.Create(direction, transform.position + direction * 0.5f, 3f);
            }

            _contracting = false;
        });
    }

    public void Update()
    {
        UpdateFireCounter();
        UpdateContractTimer();
        UpdateBurstCounter();
        if (!_contracting) _burstForce = Mathf.PerlinNoise(Time.timeSinceLevelLoad, 0f);
        for (int i = Sections.Count - 1; i >= 0; --i)
            ((SwarmSegmentBehaviour) Sections[i]).UpdateSection(transform.position, _burstForce * 1.3f - 0.3f);
    }

    private void CheckToSplit()
    {
        int afterCount = SectionCount();
        int beforeCount = afterCount + 1;
        if (beforeCount > FirstSplitThreshold && afterCount <= FirstSplitThreshold) Split();
        if (beforeCount > SecondSplitThreshold && afterCount <= SecondSplitThreshold) Split();
        if (beforeCount > ThirdSplitThreshold && afterCount <= ThirdSplitThreshold) Split();
    }

    public override void UnregisterSection(BossSectionHealthController segment)
    {
        Sections.Remove(segment);
        CheckToSplit();
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