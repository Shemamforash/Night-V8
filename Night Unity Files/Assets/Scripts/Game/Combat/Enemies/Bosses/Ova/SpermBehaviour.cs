using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Gear.Armour;
using SamsHelper.Libraries;
using UnityEngine;

public class SpermBehaviour : CanTakeDamage
{
    private static GameObject _spermPrefab;
    private bool _followPlayer;
    private Heavyshot _heavyShot;
    private Rigidbody2D _rigidbody;
    private Vector3 _targetPosition;
    private float Speed;

    protected override void Awake()
    {
        base.Awake();
        Speed = Random.Range(2f, 4f);
        HealthController.SetInitialHealth(150, this);
        ArmourController.AutoGenerateArmour();
        _heavyShot = GetComponent<Heavyshot>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _heavyShot.Initialise(1f, 0.4f, 5, 0.5f);
        _heavyShot.SetFiring(false);
    }

    public override string GetDisplayName()
    {
        return "Ahna's Bane";
    }

    private void SetFollowing(bool following)
    {
        _followPlayer = following;
        _heavyShot.SetFiring(following);
    }

    public override void MyUpdate()
    {
        base.MyUpdate();
        float distanceToTarget = transform.Distance(PlayerCombat.Position());
        if (_followPlayer)
        {
            if (distanceToTarget < 1f)
            {
                SetFollowing(false);
                Vector3 dirToPlayer = PlayerCombat.Position() - transform.position;
                dirToPlayer.Normalize();
                Vector2 locationBeyondPlayer = transform.position + dirToPlayer * Random.Range(4f, 6f);
                _targetPosition = AdvancedMaths.RandomVectorWithinRange(locationBeyondPlayer, 1f);
                return;
            }

            _targetPosition = PlayerCombat.Position();
            return;
        }

        if (transform.Distance(_targetPosition) > 0.5f) return;
        SetFollowing(true);
    }

    public static void Create()
    {
        if (_spermPrefab == null) _spermPrefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Ova/Sperm");
        Instantiate(_spermPrefab).transform.position = Vector2.zero;
    }

    public void FixedUpdate()
    {
        Vector3 dir = (_targetPosition - transform.position).normalized * Speed;
        _rigidbody.AddForce(dir);
    }
}