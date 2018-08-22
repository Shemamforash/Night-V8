using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class SpermBehaviour : MonoBehaviour
{
    private static GameObject _spermPrefab;
    private bool _followPlayer;
    private HealthController _healthController;
    private Heavyshot _heavyShot;
    private Rigidbody2D _rigidbody;
    private Vector3 _targetPosition;
    private float Speed;

    public void Awake()
    {
        Speed = Random.Range(2f, 3f);
        _healthController = new HealthController();
        _healthController.SetInitialHealth(150, null);
        _heavyShot = GetComponent<Heavyshot>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _heavyShot.Initialise(1f, 0.4f, 5, 0.5f);
        _heavyShot.SetFiring(false);
        gameObject.GetComponent<SpermSegmentBehaviour>().SetSperm(this);
        Helper.FindAllComponentsInChildren<SpermSegmentBehaviour>(transform).ForEach(s => { s.SetSperm(this); });
    }

    public bool IsDead() => _healthController.GetCurrentHealth() == 0;

    public void TakeDamage(int damage)
    {
        _healthController.TakeDamage(damage);
    }

    private void SetFollowing(bool following)
    {
        _followPlayer = following;
        _heavyShot.SetFiring(following);
    }

    public void Update()
    {
        float distanceToTarget = transform.Distance(PlayerCombat.Instance.transform.position);
        if (_followPlayer)
        {
            if (distanceToTarget < 1f)
            {
                SetFollowing(false);
                Vector3 dirToPlayer = PlayerCombat.Instance.transform.position - transform.position;
                dirToPlayer.Normalize();
                Vector2 locationBeyondPlyer = transform.position + dirToPlayer * Random.Range(4f, 6f);
                _targetPosition = AdvancedMaths.RandomVectorWithinRange(locationBeyondPlyer, 1f);
                return;
            }

            _targetPosition = PlayerCombat.Instance.transform.position;
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