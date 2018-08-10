using System.Collections.Generic;
using System.Linq;
using System.Net;
using Game.Combat.Enemies.Bosses;
using SamsHelper.Libraries;
using UnityEngine;

public class SwarmSegmentBehaviour : BossSectionHealthController
{
    private Vector2 dir;
    private Rigidbody2D _rigidBody;
    private float _force;
    private readonly List<SwarmSegmentBehaviour> _neighbors = new List<SwarmSegmentBehaviour>();


    public override void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _force = Random.Range(0.6f, 1.4f);
        dir = AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 1f);
        _rigidBody.velocity = dir;
        transform.position = AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 1f);
    }

    public override void Start()
    {
        SetBoss(SwarmBehaviour.Instance());
        base.Start();
    }

    protected override int GetInitialHealth()
    {
        return 10;
    }

    protected void FixedUpdate()
    {
        _rigidBody.velocity = dir;
    }

    public void SetNeighbors(List<BossSectionHealthController> sections)
    {
        sections.ForEach(s => _neighbors.Add((SwarmSegmentBehaviour)s));
        _neighbors.Remove(this);
    }

    private List<SwarmSegmentBehaviour> SegmentsInRange()
    {
        return new List<SwarmSegmentBehaviour>(_neighbors.Where(n => n.transform.Distance(transform) < 1));
    }

    private Vector2 ComputeAlignment(List<SwarmSegmentBehaviour> neighborsInRange)
    {
        Vector2 alignment = new Vector2();
        int neighborCount = neighborsInRange.Count;
        if (neighborCount == 0) return alignment;
        neighborsInRange.ForEach(n =>
        {
            alignment += n._rigidBody.velocity;
            ++neighborCount;
        });
        alignment /= neighborCount;
        return alignment.normalized;
    }

    private Vector2 ComputeCohesion(List<SwarmSegmentBehaviour> neighborsInRange)
    {
        Vector3 cohesion = new Vector2();
        int neighborCount = neighborsInRange.Count;
        if (neighborCount == 0) return cohesion;
        neighborsInRange.ForEach(n =>
        {
            cohesion += n.transform.position;
            ++neighborCount;
        });
        cohesion /= neighborCount;
        cohesion = new Vector2(cohesion.x - transform.position.x, cohesion.y - transform.position.y);
        return cohesion.normalized;
    }

    private Vector2 ComputeSeparation(List<SwarmSegmentBehaviour> neighborsInRange)
    {
        Vector3 separation = new Vector2();
        int neighborCount = neighborsInRange.Count;
        if (neighborCount == 0) return separation;
        neighborsInRange.ForEach(n =>
        {
            separation += n.transform.position - transform.position;
            ++neighborCount;
        });
        separation /= neighborCount;
        separation *= -1;
        return separation.normalized;
    }
    
    public void UpdateSection(Vector3 parentPosition, float burstForce)
    {
        List<SwarmSegmentBehaviour> neighborsInRange = SegmentsInRange();
        Vector2 alignment = ComputeAlignment(neighborsInRange);
        Vector2 cohesion = ComputeCohesion(neighborsInRange);
        Vector2 separation = ComputeSeparation(neighborsInRange);
        dir = alignment + cohesion + separation;
        dir.Normalize();
        dir *= _force;
        return;
        
        

        Vector3 targetPosition = parentPosition;
        dir = targetPosition - transform.position;
        dir *= _force;
        dir += dir * -burstForce;
    }
}