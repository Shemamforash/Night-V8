using System.Collections;
using System.Collections.Generic;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class SerpentBehaviour : Boss
{
    private static GameObject _serpentPrefab;
    private static SerpentBehaviour _instance;
    private static float Speed = 10f;

    private TailFollowBehaviour _head;
    private SerpentBombAttack _bombAttack;

    private Vector3 _targetPosition;
    private bool _firing, _pushing, _gettingInPosition, _canPush;
    private float _pushTimer, _beamTimer;
    private Transform _beam1Target, _beam2Target;
    private float _currentBeamAngle;


    protected override void Awake()
    {
        base.Awake();
        _bombAttack = gameObject.AddComponent<SerpentBombAttack>();
        _instance = this;
        _head = gameObject.FindChildWithName<TailFollowBehaviour>("Wing Segment (0)");
        _beam1Target = new GameObject("Beam 1 Target").transform;
        _beam2Target = new GameObject("Beam 2 Target").transform;
        _beam1Target.SetParent(transform);
        _beam2Target.SetParent(transform);
        Vector2 position = transform.position;
        _beam1Target.position = position;
        _beam2Target.position = position;
        GetNewTargetPosition();
    }

    private void UpdateBeamTargets()
    {
        float sinTime = Mathf.Sin(Time.timeSinceLevelLoad * 2.5f);
        sinTime = (sinTime + 1) / 2f;
        float angleDelta = 15f * sinTime + 5f;
        float beam1Rotation = angleDelta;
        float beam2Rotation = -angleDelta;
        _beam1Target.transform.localRotation = Quaternion.Euler(0, 0, beam1Rotation);
        _beam2Target.transform.localRotation = Quaternion.Euler(0, 0, beam2Rotation);
    }

    private IEnumerator DoBeamAttack()
    {
        _firing = true;
        float time = 1f;
        while (time > 0f)
        {
            if (!CombatManager.Instance().IsCombatActive()) yield return null;
            time -= Time.deltaTime;
            yield return null;
        }

        BeamController.Create(_beam1Target);
        BeamController.Create(_beam2Target);
        time = 5f;
        while (time > 0f)
        {
            if (!CombatManager.Instance().IsCombatActive()) yield return null;
            time -= Time.deltaTime;
            yield return null;
        }

        _beamTimer = Random.Range(5, 10);
        _firing = false;
        GetNewTargetPosition();
    }

    private void UpdateBeamTimer()
    {
        if (_firing) return;
        if (_beamTimer < 0)
        {
            GetInPosition();
            return;
        }

        _beamTimer -= Time.deltaTime;
    }

    private void GetInPosition()
    {
        if (!_gettingInPosition)
        {
            _targetPosition = AdvancedMaths.RandomVectorWithinRange(Vector2.zero, 1).normalized * 8;
            _targetPosition += PlayerCombat.Position();
            Speed = 5;
            _gettingInPosition = true;
        }

        if (Vector2.Distance(transform.position, _targetPosition) > 1f) return;
        Speed = 3;
        _gettingInPosition = false;
        StartCoroutine(DoBeamAttack());
    }

    public List<BossSectionHealthController> GetSections()
    {
        return Sections;
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

        if (prevWingCount > 35 && currentWingCount <= 35)
        {
            GameObject tailEnd = transform.FindChildWithName("Tail End").gameObject;
            LeaveFireTrail fireTrail = tailEnd.AddComponent<LeaveFireTrail>();
            fireTrail.Initialise();
            fireTrail.AddIgnoreTargets(Sections);
        }
        else if (prevWingCount > 30 && currentWingCount <= 30)
            _canPush = true;

        if (currentWingCount > 25) return;
        float timeToBomb = currentWingCount / 40f + 0.25f;
        _bombAttack.SetMinTimeToBomb(timeToBomb);
    }

    public void Update()
    {
        if (!CombatManager.Instance().IsCombatActive()) return;
        UpdateBeamTimer();
        UpdatePush();
    }

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
            PushController.Create(current.transform.position, angleA, false, 10);
            PushController.Create(current.transform.position, angleB, false, 10);
            yield return new WaitForSeconds(0.05f);
            current = current.GetChild();
        }

        _pushing = false;
        _pushTimer = Random.Range(5, 10);
    }

    private void GetNewTargetPosition()
    {
        Vector2 dir = AdvancedMaths.RandomDirection();
        _targetPosition = dir * Random.Range(5f, 7.5f) + (Vector2) PlayerCombat.Position();
    }

    public void FixedUpdate()
    {
        if (!CombatManager.Instance().IsCombatActive()) return;
        if (!_gettingInPosition)
        {
            if (_firing) _targetPosition = PlayerCombat.Position();
            else if (Vector2.Distance(transform.position, _targetPosition) < 1f) GetNewTargetPosition();
        }

        UpdateBeamTargets();
        Vector2 direction = _targetPosition.Direction(transform.position);
        RigidBody.AddForce(direction * Speed);
    }
}