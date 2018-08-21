using Game.Combat.Enemies.Bosses;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class SerpentBehaviour : Boss
{
    private float _currentAngle, _radius = 4f;
    private SerpentSegmentBehaviour Head;
    private const float Speed = 3f;
    private float _findNewTargetTime;
    private Vector3 _targetPosition;
    private static GameObject _serpentPrefab;
    private static SerpentBehaviour _instance;

    public override void Awake()
    {
        base.Awake();
        _instance = this;
        Head = transform.Find("Wing Segment (0)").GetComponent<SerpentSegmentBehaviour>();
        Head.SetNextSegment(transform, 0);
        GetComponent<Beam>().Initialise(5, 2);
    }

    public static SerpentBehaviour Instance()
    {
        return _instance;
    }

    public static void Create()
    {
        if (_serpentPrefab == null) _serpentPrefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Serpent/Serpent");
        Instantiate(_serpentPrefab).transform.position = new Vector2(10, 10);
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
        //todo steer
        Vector3 dir = (_targetPosition - transform.position).normalized * Speed;
        RigidBody.AddForce(dir);
        float rot = AdvancedMaths.AngleFromUp(Vector2.zero, RigidBody.velocity);
        Head.transform.rotation = Quaternion.Euler(0, 0, rot);
        Head.NextSegment.SetPosition(transform.position, rot);
    }
}