using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Enemies.Misc;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using UnityEngine;

public class StarfishBehaviour : Boss
{
    private readonly List<StarFishMainArmBehaviour> _arms = new List<StarFishMainArmBehaviour>();
    private static float _radiusModifier = 1f;
    private float _timeToContract;
    private static int _bombsToLaunch;
    private static StarfishBehaviour _instance;
    private bool _contracting;
    private float _ghoulSpawnRate;
    private float _timeToNextGhoul;
    private float _heavyShotTimer;
    private bool _firingShot;

    public static void Create()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Starfish/Starfish");
        Instantiate(prefab).transform.position = new Vector2(0, 0);
    }

    protected override void Awake()
    {
        base.Awake();
        _instance = this;
        SetRadiusModifier(0f);
        DOTween.To(GetRadiusModifier, SetRadiusModifier, 1f, 2f);
    }

    public void Start()
    {
        for (int i = 1; i < 6; ++i)
        {
            GameObject mainArm = gameObject.FindChildWithName("Main Arm " + i);
            _arms.Add(mainArm.GetComponent<StarFishMainArmBehaviour>());
        }

        PrewarmArms();
    }

    public static StarfishBehaviour Instance()
    {
        return _instance;
    }

    private void PrewarmArms()
    {
        for (int i = 0; i < _arms.Count; i++)
        {
            StarFishMainArmBehaviour arm = _arms[i];
            float zAngle = GetCurrentAngle(i);
            for (int j = 0; j < 150; ++j) arm.UpdateAngle(zAngle);
        }
    }

    private float GetCurrentAngle(int i)
    {
        float sinTime = Mathf.Sin(Time.timeSinceLevelLoad + 0.25f * Mathf.PerlinNoise(Time.timeSinceLevelLoad, i));
        float zAngle = sinTime * 30;
        return zAngle;
    }

    public static float GetRadiusModifier()
    {
        return _radiusModifier;
    }

    private static void SetRadiusModifier(float radiusModifier)
    {
        _radiusModifier = radiusModifier;
    }

    private void PushPulse()
    {
        PushController.Create(transform.position, 0f, 40);
        PushController.Create(transform.position, 72f, 40f);
        PushController.Create(transform.position, 144f, 40f);
        PushController.Create(transform.position, 216f, 40f);
        PushController.Create(transform.position, 288f, 40f);
    }

    private void LaunchBombs()
    {
        if (_bombsToLaunch == 0) return;
        float angleDivision = 360f / _bombsToLaunch;
        for (int i = 0; i < _bombsToLaunch; ++i)
        {
            float angleFrom = i * angleDivision;
            float angleTo = (i + 1) * angleDivision;
            float angle = Random.Range(angleFrom, angleTo);
            Vector2 randomPosition = AdvancedMaths.CalculatePointOnCircle(angle, Random.Range(3f, 5f), transform.position);
            Grenade.CreateBasic(transform.position, randomPosition);
        }
    }

    private void Contract()
    {
        _contracting = true;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(GetRadiusModifier, SetRadiusModifier, 1.2f, 1f).SetEase(Ease.OutExpo));
        sequence.Append(DOTween.To(GetRadiusModifier, SetRadiusModifier, 0.4f, 0.2f).SetEase(Ease.InBack));
        sequence.AppendCallback(PushPulse);
        sequence.AppendCallback(LaunchBombs);
        sequence.Append(DOTween.To(GetRadiusModifier, SetRadiusModifier, 1f, 3f));
        sequence.AppendCallback(ResetContract);
    }

    private void ResetContract()
    {
        _timeToContract = Random.Range(5f, 6f);
        _contracting = false;
    }

    public void Update()
    {
        if (!CombatManager.IsCombatActive()) return;
        UpdateSpawn();
        UpdateHeavyShot();
        if (_contracting) return;
        if (_timeToContract > 0f) _timeToContract -= Time.deltaTime;
        else Contract();
    }

    public void UpdateHeavyShot()
    {
        if (_firingShot) return;
        if (_heavyShotTimer > 0f)
        {
            _heavyShotTimer -= Time.deltaTime;
            return;
        }

        StartCoroutine(FireHeavyShot());
    }

    private IEnumerator FireHeavyShot()
    {
        _firingShot = true;
        int count = 50;
        float angleInterval = 180f / count;
        while (count > 0f)
        {
            float angle = angleInterval * count;
            float angleB = angle + 180;
            --count;
            float x = Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector3 dirA = new Vector2(x, y);
            x = Mathf.Cos(angleB * Mathf.Deg2Rad);
            y = Mathf.Sin(angleB * Mathf.Deg2Rad);
            Vector3 dirB = new Vector2(x, y);
            MaelstromShotBehaviour.Create(dirA, transform.position + dirA, 1.5f, false);
            MaelstromShotBehaviour.Create(dirB, transform.position + dirB, 1.5f, false);
            yield return new WaitForSeconds(0.15f);
        }

        _heavyShotTimer = Random.Range(5f, 10f);
        _firingShot = false;
    }

    private void UpdateSpawn()
    {
        if (_ghoulSpawnRate == 0) return;
        _timeToNextGhoul -= Time.deltaTime;
        if (_timeToNextGhoul > 0f) return;
        EnemyBehaviour enemy = CombatManager.SpawnEnemy(EnemyType.Ghoul, AdvancedMaths.RandomDirection() * 9);
        enemy.gameObject.AddComponent<LeaveFireTrail>().Initialise();
        _timeToNextGhoul = Random.Range(_ghoulSpawnRate, _ghoulSpawnRate * 2f);
    }

    public void FixedUpdate()
    {
        for (int i = 0; i < _arms.Count; i++)
        {
            StarFishMainArmBehaviour a = _arms[i];
            a.UpdateAngle(GetCurrentAngle(i));
        }
    }

    public override void UnregisterSection(BossSectionHealthController starFishArmBehaviour)
    {
        base.UnregisterSection(starFishArmBehaviour);
        int armCount = SectionCount();
        _ghoulSpawnRate = (-2f * armCount) / 55f + 2f;
        if (_ghoulSpawnRate < 0) _ghoulSpawnRate = 0;
        if (armCount > 35) return;
        _bombsToLaunch = Mathf.CeilToInt((35f - armCount) / 3f);
    }
}