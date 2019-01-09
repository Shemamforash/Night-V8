using System.Collections;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Gear.Armour;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;
using UnityScript.Steps;

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
        HealthController.SetInitialHealth(WorldState.ScaleValue(400), this);
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
        if (HealthController.GetCurrentHealth() == 0) return;
        Vector3 dir = (_targetPosition - transform.position).normalized * Speed;
        _rigidbody.AddForce(dir);
    }

    public override void Kill()
    {
        CombatManager.RemoveEnemy(this);
        Destroy(GetComponent<CircleCollider2D>());
        StartCoroutine(Explode());
    }

    private IEnumerator Explode()
    {
        int childCount = transform.childCount - 1;
        for (int i = 0; i < childCount; ++i)
        {
            Destroy(transform.GetChild(i).GetComponent<TailFollowBehaviour>());
        }

        while (childCount >= 0)
        {
            if (!CombatManager.IsCombatActive()) yield return null;
            Transform child = transform.GetChild(childCount);
            Vector3 childPosition = child.transform.position;
            LeafBehaviour.CreateLeaves(childPosition);
            MaelstromShotBehaviour.CreateBurst(60, childPosition, 1f, Random.Range(0, 360));
            Explosion.CreateExplosion(childPosition, 0.5f).InstantDetonate();
            Destroy(child.gameObject);
            --childCount;
            yield return new WaitForSeconds(0.25f);
        }

        Vector3 position = transform.position;
        LeafBehaviour.CreateLeaves(position);
        MaelstromShotBehaviour.CreateBurst(30, position, 1f, Random.Range(0, 360));
        Explosion.CreateExplosion(position);
        Destroy(gameObject);
    }
}