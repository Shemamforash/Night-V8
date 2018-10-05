using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Enemies.Misc;
using SamsHelper.Libraries;
using UnityEngine;

public class StarfishBehaviour : Boss
{
    private readonly List<StarFishMainArmBehaviour> _arms = new List<StarFishMainArmBehaviour>();
    private static float _radiusModifier = 1f;
    private float _timeToContract;
    private static int _bombsToLaunch;
    private static readonly List<StarFishArmBehaviour> _armSegments = new List<StarFishArmBehaviour>();
    private static StarfishBehaviour _instance;


    public static void Create()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Starfish/Starfish");
        Instantiate(prefab).transform.position = new Vector2(0, 0);
    }

    protected override void Awake()
    {
        base.Awake();
        _instance = this;
    }

    public override string GetDisplayName()
    {
        return "Starfish";
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
            for (int j = 0; j < 150; ++j)
            {
                arm.UpdateAngle(zAngle);
            }
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

    private void Contract()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(DOTween.To(GetRadiusModifier, SetRadiusModifier, 1.2f, 1f).SetEase(Ease.OutExpo));
        sequence.Append(DOTween.To(GetRadiusModifier, SetRadiusModifier, 0.4f, 0.2f).SetEase(Ease.InBack));
        sequence.AppendCallback(() =>
        {
            PushController.Create(transform.position, 0f, 40);
            PushController.Create(transform.position, 72f, 40f);
            PushController.Create(transform.position, 144f, 40f);
            PushController.Create(transform.position, 216f, 40f);
            PushController.Create(transform.position, 288f, 40f);
        });
        sequence.AppendCallback(() =>
        {
            if (_bombsToLaunch == 0) return;
            float angleDivision = 360f / _bombsToLaunch;
            for (int i = 0; i < _bombsToLaunch; ++i)
            {
                float angleFrom = i * angleDivision;
                float angleTo = (i + 1) * angleDivision;
                float angle = Random.Range(angleFrom, angleTo);
                Vector2 randomPosition = AdvancedMaths.CalculatePointOnCircle(angle, Random.Range(3f, 5f), transform.position);
                Grenade.CreateBasic(transform.position, randomPosition, true);
            }
        });
        sequence.Append(DOTween.To(GetRadiusModifier, SetRadiusModifier, 1f, 3f));
    }

    public void Update()
    {
        if (_timeToContract > 0f)
        {
            _timeToContract -= Time.deltaTime;
            return;
        }

        _timeToContract = Random.Range(5f, 6f);
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

    public static void RegisterArm(StarFishArmBehaviour starFishArmBehaviour)
    {
        _armSegments.Add(starFishArmBehaviour);
    }

    public static void UnregisterArm(StarFishArmBehaviour starFishArmBehaviour)
    {
        int armCount = _armSegments.Count;
        _armSegments.Remove(starFishArmBehaviour);
        if (armCount > 35) return;
        _bombsToLaunch = Mathf.CeilToInt((35f - armCount) / 3f);
    }
}