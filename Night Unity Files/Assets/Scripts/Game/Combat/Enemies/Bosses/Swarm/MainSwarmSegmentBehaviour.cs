using System.Collections;
using DG.Tweening;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

public class MainSwarmSegmentBehaviour : CanTakeDamage
{
    private bool _contracting;
    private static GameObject[] _swarmPrefabs;
    private Orbit _orbit;
    private float _childSpawnTimer;
    private const float BaseChildSpawnTimer = 1.5f;
    private const float ChildSpawnDecayRate = 0.99f;
    private const int InitialSwarmCount = 20;
    private bool _canFire, _canBurst, _canContract;

    protected override void Awake()
    {
        base.Awake();
        _orbit = gameObject.AddComponent<Orbit>();
        int initialHealth = WorldState.ScaleValue(1200);
        HealthController.SetInitialHealth(initialHealth, this);
        ArmourController.AutoGenerateArmour();
        SpriteFlash = gameObject.FindChildWithName<DamageSpriteFlash>("Sprite");
    }

    public override string GetDisplayName() => "The Soul of Rhallos";

    private void Start()
    {
        for (int i = 0; i < InitialSwarmCount; ++i) SpawnChild();
        _orbit.Initialise(PlayerCombat.Instance.transform, GetComponent<Rigidbody2D>().AddForce, 0.25f, 1.5f, 2.5f);
        StartCoroutine(StartSpawnChildBehaviour());
        StartCoroutine(StartFireBehaviour());
        StartCoroutine(StartContractBehaviour());
        StartCoroutine(StartBurstBehaviour());
    }

    public override void Kill()
    {
        if (SwarmBehaviour.Instance().CheckChangeToStageTwo())
        {
            HealthController.Heal(100000);
            return;
        }

        base.Kill();
        SwarmBehaviour.Instance().Kill();
    }

    protected override void TakeDamage(int damage, Vector2 direction)
    {
        float normalisedHealthBefore = HealthController.GetNormalisedHealthValue();
        base.TakeDamage(damage, direction);
        float normalisedHealthAfter = HealthController.GetNormalisedHealthValue();
        if (normalisedHealthBefore > 0.8f && normalisedHealthAfter <= 0.8f) _canFire = true;
        if (normalisedHealthBefore > 0.6f && normalisedHealthAfter <= 0.6f) _canBurst = true;
        if (normalisedHealthBefore > 0.4f && normalisedHealthAfter <= 0.4f) _canContract = true;
    }

    public void Update()
    {
        if (!CombatManager.Instance().IsCombatActive()) return;
        for (int i = SwarmSegmentBehaviour.Active.Count - 1; i >= 0; --i)
            SwarmSegmentBehaviour.Active[i].UpdateSection();
    }

    private void FixedUpdate()
    {
        SwarmSegmentBehaviour.Active.ForEach(s => s.MyFixedUpdate());
    }

    private IEnumerator StartSpawnChildBehaviour()
    {
        float spawnTimeModifier = 2f;
        while (true)
        {
            if (!_contracting && CombatManager.Instance().IsCombatActive() && SwarmSegmentBehaviour.Active.Count < 100)
            {
                float time = BaseChildSpawnTimer * spawnTimeModifier;
                spawnTimeModifier *= ChildSpawnDecayRate;
                if (spawnTimeModifier < 0.5f) spawnTimeModifier = 0.5f;
                SpawnChild();
                yield return new WaitForSeconds(time);
            }
            else
            {
                yield return null;
            }
        }
    }

    private void SpawnChild()
    {
        if (_swarmPrefabs == null) _swarmPrefabs = Resources.LoadAll<GameObject>("Prefabs/Combat/Bosses/Swarm/Segments");
        SwarmSegmentBehaviour swarm = Instantiate(_swarmPrefabs.RandomElement()).GetComponent<SwarmSegmentBehaviour>();
        swarm.SetSwarmParent(SwarmBehaviour.Instance());
    }

    private IEnumerator StartFireBehaviour()
    {
        while (true)
        {
            if (!_contracting && CombatManager.Instance().IsCombatActive() && _canFire && SwarmSegmentBehaviour.Active.Count > 0)
            {
                SwarmSegmentBehaviour swarmSegment = SwarmSegmentBehaviour.Active[0];
                swarmSegment.StartSeeking();
                float time = HealthController.GetNormalisedHealthValue() + 0.5f;
                yield return new WaitForSeconds(time);
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator StartBurstBehaviour()
    {
        while (true)
        {
            if (!_contracting && CombatManager.Instance().IsCombatActive() && _canBurst)
            {
                if (!_canBurst) yield return null;
                SwarmSegmentBehaviour.Active.ForEach(s => s.StartBurst());
                yield return DOTween.To(() => RadiusModifier, f => RadiusModifier = f, 4f, 1f).SetEase(Ease.InExpo).WaitForCompletion();
                SwarmSegmentBehaviour.Active.ForEach(s => s.Detonate());
                yield return new WaitForSeconds(1.5f);
                yield return DOTween.To(() => RadiusModifier, f => RadiusModifier = f, 1f, 0.5f).SetEase(Ease.InExpo).WaitForCompletion();
                SwarmSegmentBehaviour.Active.ForEach(s => s.EndBurst());
                yield return new WaitForSeconds(14);
            }
            else
            {
                yield return null;
            }
        }
    }

    public static float RadiusModifier = 1f;

    private IEnumerator StartContractBehaviour()
    {
        while (true)
        {
            if (CombatManager.Instance().IsCombatActive() && _canContract)
            {
                _contracting = true;
                yield return DOTween.To(() => RadiusModifier, f => RadiusModifier = f, 0.1f, 1f).SetEase(Ease.InExpo).WaitForCompletion();
                yield return new WaitForSeconds(0.5f);
                float angleInterval = 360 / 3f;
                for (float angle = 0f; angle < 360f; angle += angleInterval)
                {
                    Vector3 direction = AdvancedMaths.CalculatePointOnCircle(angle, 1, Vector2.zero);
                    MaelstromShotBehaviour.Create(direction, transform.position + direction * 0.5f, 3f, false);
                }

                yield return new WaitForSeconds(0.2f);
                yield return DOTween.To(() => RadiusModifier, f => RadiusModifier = f, 1f, 0.5f).SetEase(Ease.OutBack).WaitForCompletion();
                _contracting = false;
                yield return new WaitForSeconds(Random.Range(3f, 5f));
            }
            else
            {
                yield return null;
            }
        }
    }
}