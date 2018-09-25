using Game.Combat.Enemies.Bosses;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class SerpentBehaviour : Boss
{
    private const float Speed = 3f;
    private float _findNewTargetTime;
    private Vector3 _targetPosition;
    private static GameObject _serpentPrefab;
    private static SerpentBehaviour _instance;

    public override void Awake()
    {
        base.Awake();
        _instance = this;
        GetComponent<Beam>().Initialise(5, 2);
    }

    public static SerpentBehaviour Instance()
    {
        return _instance;
    }

    public static void Create()
    {
        if (_serpentPrefab == null) _serpentPrefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Serpent/Serpent");
        Instantiate(_serpentPrefab).transform.position = Vector2.zero;
    }

    public override void UnregisterSection(BossSectionHealthController section)
    {
        int prevWingCount = SectionCount();
        base.UnregisterSection(section);
        if (SectionCount() > 20 || prevWingCount != 20) return;
        AddEggAttack();
    }

    private void AddEggAttack()
    {
        gameObject.AddComponent<SerpentEggs>().Initialise(5f, 10f, transform.FindChildWithName("Tail End"));
    }

    public void Update()
    {
        float distanceToTarget = Vector2.Distance(transform.position, _targetPosition);
        if (_findNewTargetTime <= 0f || distanceToTarget < 0.5f)
        {
            _findNewTargetTime = Random.Range(3f, 7f);
            Vector3 dir = (PlayerCombat.Instance.transform.position - transform.position).normalized;
            _targetPosition = AdvancedMaths.RandomVectorWithinRange(PlayerCombat.Instance.transform.position + dir * 5f, 3f);
        }

        _findNewTargetTime -= Time.deltaTime;
    }

    public void FixedUpdate()
    {
        Vector3 dir = (_targetPosition - transform.position).normalized * Speed;
        RigidBody.AddForce(dir);
    }
}