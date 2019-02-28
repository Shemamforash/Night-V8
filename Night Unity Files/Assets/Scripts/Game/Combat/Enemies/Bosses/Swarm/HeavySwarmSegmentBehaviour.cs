using System.Collections.Generic;
using Game.Combat.Enemies.Bosses;
using SamsHelper.Libraries;
using UnityEngine;

public class HeavySwarmSegmentBehaviour : BossSectionHealthController
{
    private Rigidbody2D _rigidBody;
    private float _burstTimer;
    private const float OrbitRadius = 2f;
    public static readonly List<HeavySwarmSegmentBehaviour> Active = new List<HeavySwarmSegmentBehaviour>();
    private float _angleOffset;
    private Vector2 _direction;

    protected override void Awake()
    {
        base.Awake();
        _rigidBody = GetComponent<Rigidbody2D>();
        SetBoss(SwarmBehaviour.Instance());
        ResetBurstTimer();
        ArmourController.AutoGenerateArmour();
        SpriteFlash = gameObject.FindChildWithName<DamageSpriteFlash>("Sprite");
        Active.Add(this);
    }

    public void SetAngleOffset(float angleOffset) => _angleOffset = angleOffset;

    private void OnDestroy()
    {
        Active.Remove(this);
    }

    public override string GetDisplayName() => "The Soul of Rha";

    protected override int GetInitialHealth() => 300;

    public void Update()
    {
        UpdateBurst();
    }

    private void ResetBurstTimer() => _burstTimer = 2f + 2f * HealthController.GetNormalisedHealthValue();

    private void UpdateBurst()
    {
        _burstTimer -= Time.deltaTime;
        if (_burstTimer > 0) return;
        for (int i = 0; i < 6; ++i)
        {
            float angle = 360f / 6 * i;
            Vector2 position = AdvancedMaths.CalculatePointOnCircle(angle, 0.6f, transform.position);
            Vector2 direction = (Vector2) transform.position - position;
            MaelstromShotBehaviour.Create(direction, position, 1f, false, false);
        }

        ResetBurstTimer();
    }

    protected void FixedUpdate()
    {
        Vector2 parentPosition = SwarmBehaviour.Instance().transform.position;
        float angle = Time.timeSinceLevelLoad * 10 + _angleOffset;
        Vector3 targetPosition = AdvancedMaths.CalculatePointOnCircle(angle, OrbitRadius, parentPosition);
        _rigidBody.MovePosition(targetPosition);
    }
}