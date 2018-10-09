using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using UnityEngine;

public class SwarmBehaviour : Boss
{
    private static readonly List<SwarmBehaviour> _swarms = new List<SwarmBehaviour>();
    private const float MoveSpeed = 0.33f;
    private const int SwarmCount = 500;
    private float _fireCounter;
    private float _fireCounterMin, _fireCounterMax;
    private float _contractTimer;
    private bool _contracting;
    private float _burstForce;
    private static GameObject _swarmPrefab;
    private float _burstCounter;

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
        else
        {
            inheritedChildren.ForEach(s => { s.SetSwarmParent(this); });
        }

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
        _swarms.Add(this);
        _fireCounter = Random.Range(_fireCounterMin, _fireCounterMax);
        _contractTimer = Random.Range(8f, 12f);
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
        Sections.RemoveAt(0);
    }

    private void UpdateBurstCounter()
    {
        if (SectionCount() > 100) return;
        if (_burstCounter < 0f) return;
        _burstCounter -= Time.deltaTime;
        if (_burstCounter > 0f) return;
        Sections.ForEach(s => ((SwarmSegmentBehaviour) s).StartBurst());
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(3f);
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
            float angleIntervals = 1f / 20f * SectionCount() + 5;
            for (float angle = 0f; angle < 360f; angle += angleIntervals)
            {
                Vector3 direction = new Vector2();
                direction.x = Mathf.Cos(angle);
                direction.y = Mathf.Sin(angle);
                MaelstromShotBehaviour.Create(direction, transform.position + direction * 0.5f, 3f);
            }

            _contracting = false;
        });
    }

    public void FixedUpdate()
    {
        Vector2 mousePosition = Helper.MouseToWorldCoordinates();
        Vector2 dir = (mousePosition - (Vector2) transform.position).normalized;
        RigidBody.AddForce(dir * MoveSpeed);
    }

    public void Update()
    {
        UpdateFireCounter();
        UpdateContractTimer();
        UpdateBurstCounter();
        if (!_contracting) _burstForce = Mathf.PerlinNoise(Time.timeSinceLevelLoad, 0f);
        for (int i = Sections.Count - 1; i >= 0; --i)
        {
            ((SwarmSegmentBehaviour) Sections[i]).UpdateSection(transform.position, _burstForce * 1.3f - 0.3f);
        }
    }

    public override void UnregisterSection(BossSectionHealthController segment)
    {
        int beforeCount = SectionCount();
        Sections.Remove(segment);
        int afterCount = SectionCount();
        if (beforeCount > SwarmCount / 2 && afterCount <= SwarmCount / 2) Split();
        RecalculateFireTimer();
        if (Sections.Count != 0) return;
        if (_swarms.Count == 1)
        {
            _swarms.Remove(this);
            Kill();
        }
        else Destroy(gameObject);
    }

    public static List<CanTakeDamage> GetAllSegments()
    {
        List<CanTakeDamage> segments = new List<CanTakeDamage>();
        _swarms.ForEach(s => { segments.AddRange(s.Sections); });
        return segments;
    }
}