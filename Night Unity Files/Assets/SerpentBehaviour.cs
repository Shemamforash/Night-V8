using System.Collections.Generic;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class SerpentBehaviour : SerpentSegmentBehaviour
{
    private float _currentAngle, _radius = 4f;
    private GameObject Head;
    private const float Speed = 3f;
    private Rigidbody2D _rigidbody;
    private readonly List<WingHealthScript> _wings = new List<WingHealthScript>();
    private static SerpentBehaviour _instance;
    private float _findNewTargetTime;
    private Vector3 _targetPosition;
    private static GameObject _serpentPrefab;

    public void Awake()
    {
        _instance = this;
        Head = transform.Find("Head").gameObject;
        _rigidbody = GetComponent<Rigidbody2D>();
        SetNextSegment(transform, 0);
        GetComponent<Beam>().Initialise(transform.FindChildWithName("Beam Target"), 5, 2, 2f);
    }

    public static void Create()
    {
        if (_serpentPrefab == null) _serpentPrefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Serpent/Serpent");
        Instantiate(_serpentPrefab).transform.position = new Vector2(10, 10);
    }
    
    public static void RegisterWing(WingHealthScript wing)
    {
        _instance._wings.Add(wing);
    }

    public static void UnregisterWing(WingHealthScript wing)
    {
        int prevWingCount = _instance._wings.Count;
        _instance._wings.Remove(wing);
        if(prevWingCount == 1) Destroy(_instance.gameObject);
        if (_instance._wings.Count > 20 || prevWingCount != 20) return;
        _instance.AddEggAttack();
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
            _findNewTargetTime = Random.Range(5f, 10f);
//            Vector3 dir = (Helper.MouseToWorldCoordinates() - transform.position).normalized;
            Vector3 dir = (PlayerCombat.Instance.transform.position - transform.position).normalized;
            _targetPosition = AdvancedMaths.RandomVectorWithinRange(Helper.MouseToWorldCoordinates() + dir * 6f, 3f);
        }

        _findNewTargetTime -= Time.deltaTime;
    }
    
    public void FixedUpdate()
    {
        //todo steer
        Vector3 dir = (_targetPosition - transform.position).normalized * Speed;
        _rigidbody.AddForce(dir);
        float rot = AdvancedMaths.AngleFromUp(Vector2.zero, _rigidbody.velocity);
        Head.transform.rotation = Quaternion.Euler(0, 0, rot);
        NextSegment.SetPosition(transform.position, rot);
    }
}