using System.Collections;
using System.Collections.Generic;
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
    private float _beamTimer;
    private float _distanceToPlayer;
    private bool _firing;
    private Orbit _orbit;
    private bool _canPush;
    private bool _dropBombs;
    private TailFollowBehaviour _head;

    protected override void Awake()
    {
        base.Awake();
        _orbit = gameObject.AddComponent<Orbit>();
        _orbit.Initialise(PlayerCombat.Instance.transform, v => RigidBody.AddForce(v), Speed, 4, 6);
        _instance = this;
        _head = gameObject.FindChildWithName<TailFollowBehaviour>("Wing Segment (0)");
    }

    public override string GetDisplayName()
    {
        return "Serpent";
    }

    private void DoBeamAttack()
    {
        if (_firing) return;
        _firing = true;
        SkillAnimationController.Create(transform, "Beam", 1f, () =>
        {
            BeamController.Create(transform);
            ResetBeamTimer();
        });
    }

    private void ResetBeamTimer()
    {
        _beamTimer = Random.Range(5, 10);
        _firing = false;
    }

    private void UpdateBeamTimer()
    {
        CheckToFireBeam();
        if (_beamTimer < 0) return;
        _beamTimer -= Time.deltaTime;
    }

    public List<BossSectionHealthController> GetSections()
    {
        return Sections;
    }

    private void CheckToFireBeam()
    {
        if (_beamTimer > 0 || _firing) return;
        if (_distanceToPlayer < 5f)
        {
            DoBeamAttack();
        }
        else if (Vector2.Distance(_targetPosition, PlayerCombat.Instance.transform.position) < 5f)
        {
            Vector3 dirVector = AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 1);
            _targetPosition = PlayerCombat.Instance.transform.position + dirVector * 7;
        }
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
        int currentWingCount = SectionCount();

        if (prevWingCount > 30 && currentWingCount <= 30)
        {
            GameObject tailEnd = transform.FindChildWithName("Tail End").gameObject;
            tailEnd.AddComponent<LeaveFireTrail>().Initialise(10);
        }
        else if (prevWingCount > 20 && currentWingCount <= 20)
            _canPush = true;
        else if (prevWingCount > 10 && currentWingCount <= 10)
            gameObject.AddComponent<SerpentBombAttack>();
    }

    public void Update()
    {
//        UpdateDistanceToPlayer();
//        UpdateBeamTimer();
//        UpdateTargetPosition();
        UpdatePush();
    }

    private float _pushTimer;
    private bool _pushing;

    private void UpdatePush()
    {
        if (!_canPush || _pushing) return;
        _pushTimer -= Time.deltaTime;
        if (_pushTimer > 0f) return;
        StartCoroutine(DoPush());
    }

    private IEnumerator DoPush()
    {
        _pushing = true;
        TailFollowBehaviour current = _head;
        while (current != null)
        {
            float angleA = AdvancedMaths.AngleFromUp(Vector2.zero, current.transform.right);
            float angleB = angleA + 180;
            PushController.Create(current.transform.position, angleA);
            PushController.Create(current.transform.position, angleB);
            yield return new WaitForSeconds(0.05f);
            current = current.GetChild();
        }

        _pushing = false;
        _pushTimer = Random.Range(5, 10);
    }

    private void UpdateTargetPosition()
    {
        if (!_firing) return;
        _targetPosition = PlayerCombat.Instance.transform.position;
    }

    private void UpdateDistanceToPlayer()
    {
        _distanceToPlayer = Vector2.Distance(transform.position, PlayerCombat.Instance.transform.position);
    }
}