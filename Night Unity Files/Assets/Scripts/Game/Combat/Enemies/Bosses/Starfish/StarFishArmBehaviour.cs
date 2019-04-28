using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using UnityEngine;

public class StarFishArmBehaviour : BossSectionHealthController
{
    private StarFishArmBehaviour _nextArm;
    private readonly Queue<Vector3> _parentPositions = new Queue<Vector3>();
    private readonly Queue<float> _parentRotations = new Queue<float>();
    private float _radius;
    private float _offset;
    private const float DampingModifier = 0.98f;
    private float Damping;
    private Transform _prevSegment;
    private int _distance;
    private bool _dead;

    public void Start()
    {
        SetBoss(StarfishBehaviour.Instance());
        ArmourController.AutoGenerateArmour();
    }

    protected override int GetInitialHealth()
    {
        return (int) (15f * (20 - _distance) / 2f);
    }

    public bool Dead() => _dead;

    public StarFishArmBehaviour NextArm() => _nextArm;

    public void SetPosition(float angle)
    {
        float newAngle = _offset + angle * Damping;
        Vector2 pos = AdvancedMaths.CalculatePointOnCircle(newAngle, _radius * StarfishBehaviour.GetRadiusModifier(), Vector2.zero);
        _parentPositions.Enqueue(pos);
        _parentRotations.Enqueue(angle);
        if (_parentPositions.Count < 10) return;
        Vector3 newPosition = _parentPositions.Dequeue();
        transform.position = newPosition;
        float rot = AdvancedMaths.AngleFromUp(_prevSegment.position, transform.position);
        transform.rotation = Quaternion.Euler(0, 0, rot);

        SetNextSegmentPosition(_parentRotations.Dequeue());
    }

    public void SetOffset(float offset, Transform root, Transform prevSegment, int i, float damping)
    {
        _distance = i;
        UpdateInitialHealth();
        _radius = 0.65f + i * 0.25f;
        _offset = offset;
        Damping = damping;
        _prevSegment = prevSegment;
        transform.position = root.transform.position;
        Transform wingObject = root.Find("Arm " + i);
        if (wingObject == null) return;
        _nextArm = wingObject.GetComponent<StarFishArmBehaviour>();
        if (_nextArm == null) return;
        _nextArm.SetOffset(offset, root, transform, i + 1, damping * DampingModifier);
    }

    private void SetNextSegmentPosition(float angle)
    {
        if (_nextArm == null) return;
        _nextArm.SetPosition(angle);
    }

    public override void Kill()
    {
        _dead = true;
        float opacity = Damping / 3f;
        GetComponent<SpriteRenderer>().DOFade(opacity, 1f);
        Destroy(GetComponent<PolygonCollider2D>());
        Destroy(GetComponent<DamageSpriteFlash>());
        CombatManager.Instance().RemoveEnemy(this);
        Parent.UnregisterSection(this);
        LeafBehaviour.CreateLeaves(transform.position);
    }

    public override string GetDisplayName()
    {
        return "Hythinea's Cruelty";
    }
}