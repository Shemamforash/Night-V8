using DG.Tweening;
using Game.Combat.Enemies.Bosses;
using SamsHelper.Libraries;
using UnityEngine;

public class SwarmBehaviour : Boss
{
    private static SwarmBehaviour _instance;
    private const float MoveSpeed = 0.33f;
    public int SwarmCount = 100;
    private float _fireCounter;
    private float _fireCounterMin, _fireCounterMax;
    private float _contractTimer;
    private bool _contracting;
    private float _burstForce;

    public static void Create()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Swarm/Swarm Boss");
        Instantiate(prefab).transform.position = new Vector2(0, 0);
    }
    
    public override void Awake()
    {
        base.Awake();
        _instance = this;
        GameObject swarmPrefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Swarm/Swarm Segment");
        for (int i = 0; i < SwarmCount; ++i)
        {
            Instantiate(swarmPrefab).transform.SetParent(transform);
        }

        RecalculateFireTimer();
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
        _fireCounter -= Time.deltaTime;
        if (_fireCounter > 0f) return;
        _fireCounter = Random.Range(_fireCounterMin, _fireCounterMax);
        if (SectionCount() == 0) return;
        SwarmSegmentBehaviour swarmSegment = (SwarmSegmentBehaviour) Sections[0];
        swarmSegment.StartSeeking();
        Sections.RemoveAt(0);
    }

    private void UpdateContractTimer()
    {
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
                Vector2 direction = new Vector2();
                direction.x = Mathf.Cos(angle);
                direction.y = Mathf.Sin(angle);
                MaelstromShotBehaviour.Create(direction, transform.position);
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
        if (!_contracting) _burstForce = Mathf.PerlinNoise(Time.timeSinceLevelLoad, 0f);
        Sections.ForEach(s => ((SwarmSegmentBehaviour) s).UpdateSection(transform.position, _burstForce * 1.3f - 0.3f));
    }

    public override void UnregisterSection(BossSectionHealthController segment)
    {
        base.UnregisterSection(segment);
        RecalculateFireTimer();
    }

    public static SwarmBehaviour Instance()
    {
        return _instance;
    }
}