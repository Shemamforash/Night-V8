using Boo.Lang;
using DG.Tweening;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Generation;
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
    private float _angleOffset;
    private float _force;
    private const float SeekSpeed = 4f;
    private float _seekLifeTime = 5f;
    private float _rotateSpeed;
    public static readonly List<SwarmSegmentBehaviour> Active = new List<SwarmSegmentBehaviour>();

    private enum SwarmState
    {
        Following,
        Seeking,
        Bursting,
        Collapsing
    }

    private SwarmState _currentState = SwarmState.Following;

    protected override void Awake()
    {
        base.Awake();
        _rigidBody = GetComponent<Rigidbody2D>();
        transform.position = SwarmBehaviour.Instance().transform.position;
        _angleOffset = Random.Range(0, 360f);
        _force = Random.Range(1f, 1.5f);
        _rotateSpeed = Random.Range(360f, 720f);
        if (Helper.RollDie(0, 2)) _rotateSpeed = -_rotateSpeed;
        ArmourController.AutoGenerateArmour();
        SpriteFlash = gameObject.FindChildWithName<DamageSpriteFlash>("Sprite");
        Active.Add(this);
        CombatManager.RemoveEnemy(this);
    }

    public override string GetDisplayName() => "";

    private void OnDestroy() => Active.Remove(this);

    public void SetSwarmParent(SwarmBehaviour swarmParent)
    {
        transform.SetParent(swarmParent.transform);
        SetBoss(swarmParent);
    }

    protected override int GetInitialHealth() => WorldState.ScaleValue(30);

    protected void FixedUpdate()
    {
        SpriteFlash.transform.Rotate(0, 0, _rotateSpeed * Time.fixedDeltaTime);
        switch (_currentState)
        {
            case SwarmState.Seeking:
                _rigidBody.AddForce(dir);
                break;
            case SwarmState.Following:
                _rigidBody.MovePosition(CalculateTargetPosition());
                break;
        }
    }

    public void UpdateSection(float burstForce)
    {
        switch (_currentState)
        {
            case SwarmState.Following:
                Follow(burstForce);
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
        explosion.SetBurn();
        explosion.InstantDetonate();
        Kill();
    }

    private const float OrbitRadius = 1f;

    private void Follow(float burstForce)
    {
        Vector3 parentPosition = SwarmBehaviour.Instance().transform.position;
        Vector3 targetPosition = CalculateTargetPosition();
        Vector2 burstDir = (transform.position - parentPosition).normalized * burstForce;
        Vector2 dirToTarget = (targetPosition - transform.position).normalized * _force;
        dir = dirToTarget + burstDir;
    }

    private Vector2 CalculateTargetPosition()
    {
        Vector3 parentPosition = SwarmBehaviour.Instance().transform.position;
        return AdvancedMaths.CalculatePointOnCircle((Time.timeSinceLevelLoad + _angleOffset) * 30, OrbitRadius, parentPosition);
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
        Vector2 randomDir = AdvancedMaths.RandomDirection();
        Vector2 targetPosition = randomDir * Random.Range(2.5f, 4f);
        targetPosition += (Vector2) Parent.transform.position;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_rigidBody.DOMove(targetPosition, Random.Range(1f, 2f)).SetEase(Ease.InExpo));
        sequence.AppendCallback(() =>
        {
            Explosion explosion = Explosion.CreateExplosion(transform.position, Random.Range(0.5f, 1f));
            explosion.AddIgnoreTargets(SwarmBehaviour.GetAllSegments());
            explosion.Detonate();
        });
        sequence.AppendInterval(1f);
        sequence.Append(_rigidBody.DOMove(CalculateTargetPosition(), 1).SetEase(Ease.InExpo));
        sequence.AppendCallback(() => _currentState = SwarmState.Following);
    }

    public void Collapse(Vector2 position)
    {
        _currentState = SwarmState.Collapsing;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_rigidBody.DOMove(position, Random.Range(1f, 2f)).SetEase(Ease.InExpo));
        sequence.AppendCallback(() => { _currentState = SwarmState.Following; });
    }
}