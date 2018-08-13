using DG.Tweening;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class SwarmSegmentBehaviour : BossSectionHealthController
{
    private Vector2 dir;
    private Rigidbody2D _rigidBody;
    private float _aOffset, _rOffset;
    private float _force;
    private float _followSpeed;
    [SerializeField] private float _seekSpeed = 3f;
    private bool _seeking;
    private float _seekLifeTime = 5f;

    public override void Awake()
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
        if (_seeking) _rigidBody.AddForce(dir);
        else _rigidBody.velocity = dir;
    }

    public void UpdateSection(Vector3 parentPosition, float burstForce)
    {
        if (_seeking) return;
        Follow(parentPosition, burstForce);
    }

    public void Update()
    {
        Seek();
    }

    private void Seek()
    {
        if (!_seeking) return;
        _seekLifeTime -= Time.deltaTime;
        Vector3 playerPosition = PlayerCombat.Instance.transform.position;
        Vector2 desiredVelocity = playerPosition - transform.position;
        desiredVelocity.Normalize();
        desiredVelocity *= _seekSpeed;
        dir = desiredVelocity - _rigidBody.velocity;
        if (_seekLifeTime > 0f && playerPosition.Distance(transform.position) > 0.5f) return;
        switch (Random.Range(0, 3))
        {
            case 0:
                Explosion.CreateExplosion(transform.position, 3, 0.25f).InstantDetonate();
                break;
            case 1:
                FireBehaviour.Create(transform.position, 1);
                break;
            case 2:
                DecayBehaviour.Create(transform.position);
                break;
        }

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
        _seeking = true;
        Vector2 swarmDir = SwarmBehaviour.Instance().GetComponent<Rigidbody2D>().velocity;
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
}