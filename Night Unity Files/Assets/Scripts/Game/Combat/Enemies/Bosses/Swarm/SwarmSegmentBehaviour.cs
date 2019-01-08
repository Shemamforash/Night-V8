using DG.Tweening;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

public class SwarmSegmentBehaviour : BossSectionHealthController
{
    private Vector2 dir;
    private Rigidbody2D _rigidBody;
    private float _aOffset, _rOffset;
    private float _force;
    private float _followSpeed;
    private const float SeekSpeed = 4f;
    private float _seekLifeTime = 5f;
    private float _rotateSpeed;

    private enum SwarmState
    {
        Following,
        Seeking,
        Bursting
    }

    private SwarmState _currentState = SwarmState.Following;

    protected override void Awake()
    {
        base.Awake();
        _rigidBody = GetComponent<Rigidbody2D>();
        dir = AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 1f);
        _rigidBody.velocity = dir;
        transform.position = AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 1f);
        _aOffset = Random.Range(0, 360f);
        _rOffset = Random.Range(0f, 360f);
        _force = Random.Range(0.7f, 1.3f);
        _followSpeed = Random.Range(5f, 10f);
        _rotateSpeed = Random.Range(360f, 720f);
        if (Helper.RollDie(0, 2)) _rotateSpeed = -_rotateSpeed;
        ArmourController.AutoGenerateArmour();
        SpriteFlash = gameObject.FindChildWithName<DamageSpriteFlash>("Sprite");
    }

    public override string GetDisplayName()
    {
        return "Swarm";
    }

    public void SetSwarmParent(SwarmBehaviour swarmParent)
    {
        transform.SetParent(swarmParent.transform);
        SetBoss(swarmParent);
    }

    protected override int GetInitialHealth()
    {
        return WorldState.ScaleValue(Random.Range(10, 20));
    }

    protected void FixedUpdate()
    {
        SpriteFlash.transform.Rotate(0, 0, _rotateSpeed * Time.fixedDeltaTime);
        if (_currentState == SwarmState.Seeking) _rigidBody.AddForce(dir);
        else _rigidBody.velocity = dir;
    }

    public void UpdateSection(Vector3 parentPosition, float burstForce)
    {
        switch (_currentState)
        {
            case SwarmState.Following:
                Follow(parentPosition, burstForce);
                break;
            case SwarmState.Seeking:
                Seek();
                break;
        }
    }

    public override void TakeExplosionDamage(int damage, Vector2 direction, float radius)
    {
        damage /= 10;
        if (damage < 1) damage = 1;
        base.TakeExplosionDamage(damage, direction, radius);
    }

    private void Seek()
    {
        _seekLifeTime -= Time.deltaTime;
        Vector3 playerPosition = PlayerCombat.Position();
        Vector2 desiredVelocity = playerPosition - transform.position;
        desiredVelocity.Normalize();
        desiredVelocity *= SeekSpeed;
        dir = desiredVelocity - _rigidBody.velocity;
        if (_seekLifeTime > 0f && playerPosition.Distance(transform.position) > 0.5f) return;
        Explosion explosion = Explosion.CreateExplosion(transform.position);
        explosion.AddIgnoreTargets(SwarmBehaviour.GetAllSegments());
        switch (Random.Range(0, 4))
        {
            case 0:
                explosion.SetBurn();
                break;
            case 1:
                explosion.SetDecay();
                break;
            case 2:
                explosion.SetSicken();
                break;
        }

        explosion.InstantDetonate();
        Kill();
    }

    private void Follow(Vector3 parentPosition, float burstForce)
    {
        float radius = (Mathf.Sin((Time.timeSinceLevelLoad + _rOffset) * _followSpeed) + 1f) / 2f;
        Vector2 dirToParent = parentPosition - transform.position;
        float sqrDistToParent = dirToParent.sqrMagnitude;
        Vector3 targetPosition;
        Vector2 burstDir = Vector2.zero;
        if (sqrDistToParent > 4) targetPosition = parentPosition;
        else
        {
            targetPosition = AdvancedMaths.CalculatePointOnCircle((Time.timeSinceLevelLoad + _aOffset) * 30, radius, parentPosition);
            burstDir = (transform.position - parentPosition).normalized * burstForce;
        }

        Vector2 dirToTarget = (targetPosition - transform.position).normalized * _force;
        dir = dirToTarget + burstDir;
    }

    public void StartSeeking()
    {
        if (_currentState != SwarmState.Following) return;
        _currentState = SwarmState.Seeking;
        Vector2 swarmDir = Parent.GetComponent<Rigidbody2D>().velocity;
        float x = swarmDir.y;
        float y = -swarmDir.x;
        if (Random.Range(0, 2) == 0)
        {
            x = -x;
            y = -y;
        }

        swarmDir.x = x;
        swarmDir.y = y;
        _rigidBody.AddForce(swarmDir.normalized * 100f);
        transform.DOScale(0.4f, _seekLifeTime).SetEase(Ease.InOutFlash, 10, -1);
    }

    public void StartBurst()
    {
        if (_currentState != SwarmState.Following) return;
        _currentState = SwarmState.Bursting;
        Vector2 targetPosition = AdvancedMaths.RandomPointInCircle(5f) + (Vector2) Parent.transform.position;
        Vector2 origin = transform.position;
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(Random.Range(0.5f, 1f));
        sequence.Append(_rigidBody.DOMove(targetPosition, Random.Range(1f, 2f)).SetEase(Ease.InExpo));
        sequence.AppendCallback(() =>
        {
            Explosion explosion = Explosion.CreateExplosion(transform.position, Random.Range(0.5f, 1f));
            explosion.AddIgnoreTargets(SwarmBehaviour.GetAllSegments());
            explosion.Detonate();
        });
        sequence.AppendInterval(1f);
        sequence.Append(_rigidBody.DOMove(origin, 1).SetEase(Ease.InExpo));
        sequence.AppendCallback(() => _currentState = SwarmState.Following);
    }
}