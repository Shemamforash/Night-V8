using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Enemies.Bosses.Starfish;
using Game.Combat.Enemies.Misc;
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
    private StarfishGhoulSpawn _ghoulSpawn;
    private StarfishSpreadFire _spreadFire;


    public static void Create()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Starfish/Starfish");
        Instantiate(prefab).transform.position = new Vector2(0, 0);
    }

    protected override void Awake()
    {
        base.Awake();
        _instance = this;
        _ghoulSpawn = gameObject.AddComponent<StarfishGhoulSpawn>();
        _spreadFire = gameObject.AddComponent<StarfishSpreadFire>();
    }

    public void Start()
    {
        for (int i = 1; i < 6; ++i)
        {
            GameObject mainArm = gameObject.FindChildWithName("Main Arm " + i);
            _arms.Add(mainArm.GetComponent<StarFishMainArmBehaviour>());
        }

        PreWarmArms();
    }

    public static StarfishBehaviour Instance()
    {
        return _instance;
    }

    private void PreWarmArms()
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
        //debug.log("j");
        PushController.Create(transform.position, 0f, false, 40);
        PushController.Create(transform.position, 72f, false, 40f);
        PushController.Create(transform.position, 144f, false, 40f);
        PushController.Create(transform.position, 216f, false, 40f);
        PushController.Create(transform.position, 288f, false, 40f);
        //debug.log("k");
    }

    private void LaunchBombs()
    {
        //debug.log("h");
        if (_bombsToLaunch == 0) return;
        float angleDivision = 360f / _bombsToLaunch;
        for (int i = 0; i < _bombsToLaunch; ++i)
        {
            float angleFrom = i * angleDivision;
            float angleTo = (i + 1) * angleDivision;
            float angle = Random.Range(angleFrom, angleTo);
            float x = Mathf.Cos(angle * Mathf.Deg2Rad);
            float y = Mathf.Sin(angle * Mathf.Deg2Rad);
            Vector3 direction = new Vector2(x, y) * Random.Range(3f, 5f);
            Grenade.CreateBasic(transform.position, direction);
        }

        //debug.log("i");
    }

    private void Contract()
    {
        if (_contracting) return;
        if (_timeToContract > 0f) _timeToContract -= Time.deltaTime;
        _contracting = true;
        Sequence sequence = DOTween.Sequence();
//        DOTween.Init(true, true, LogBehaviour.Verbose);
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
        _ghoulSpawn.UpdateGhoulSpawn(SectionCount());
        _spreadFire.UpdateSpreadFire();
        Contract();
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
        int armCountBefore = SectionCount();
        base.UnregisterSection(starFishArmBehaviour);
        int armCountAfter = SectionCount();

        if (armCountBefore > 50 && armCountAfter <= 50)
            _spreadFire.StartTier1();
        else if (armCountBefore > 25 && armCountAfter <= 25)
            _spreadFire.StartTier2();

        if (armCountAfter > 35) return;
        _bombsToLaunch = Mathf.CeilToInt((35f - armCountAfter) / 3f);
    }
}