using System.Collections;
using System.Collections.Generic;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

public class WormBehaviour : Boss, ITakeDamageInterface
{
    private float _timeToNextWorm, _timeToNextSac;
    private static readonly List<WormBodyBehaviour> _worms = new List<WormBodyBehaviour>();
    private static GameObject _prefab;
    private readonly HealthController _healthController = new HealthController();
    private static WormBehaviour _instance;
    private readonly ObjectPool<WormSacBehaviour> _wormSacs = new ObjectPool<WormSacBehaviour>("Sacs", "Prefabs/Combat/Bosses/Worm/Worm Sac");
    private bool _spawning;

    public override void Awake()
    {
        base.Awake();
        _instance = this;
        _healthController.SetInitialHealth(1000, this);
    }

    public static void Create()
    {
        Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Bosses/Worm/Worm"));
    }

    public override void UnregisterSection(BossSectionHealthController section)
    {
        Sections.Remove(section);
    }

    public void Update()
    {
        if (!CombatManager.IsCombatActive()) return;
        MyUpdate();
    }

    public void OnDestroy()
    {
        _wormSacs.Clear();
    }

    private void TrySpawnWorm()
    {
        _timeToNextWorm -= Time.deltaTime;
        if (_timeToNextWorm > 0f) return;
        Vector2 wormPosition = PlayerCombat.Instance.transform.position;
        if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Worm/Worm Body");
        WormBodyBehaviour newWorm = Instantiate(_prefab).GetComponent<WormBodyBehaviour>();
        newWorm.Initialise(wormPosition);
        _worms.Add(newWorm);
        _timeToNextWorm = Random.Range(8f, 12f);
    }

    private void TrySpawnSacs()
    {
        if (_spawning) return;
        _timeToNextSac -= Time.deltaTime;
        if (_timeToNextSac > 0f) return;
        StartCoroutine(SpawnSacs());
    }

    private Vector2 GetEmptySpaceNearby(List<Transform> others, float maxDistance, float minDistance, Vector2 centre)
    {
        Vector2 emptyPosition = Vector2.zero;
        bool tooClose = true;
        int iterations = 100;
        while (tooClose && iterations > 0)
        {
            tooClose = false;
            emptyPosition = AdvancedMaths.RandomVectorWithinRange(centre, maxDistance);
            foreach (Transform other in others)
            {
                float distance = emptyPosition.Distance(other.transform.position);
                if (distance > minDistance) continue;
                tooClose = true;
            }

            --iterations;
        }

        return emptyPosition;
    }

    private IEnumerator SpawnSacs()
    {
        _spawning = true;
        List<Transform> occupiedPositions = new List<Transform>();
        _wormSacs.Active().ForEach(sac => occupiedPositions.Add(sac.transform));

        Vector2 emptyPosition = GetEmptySpaceNearby(occupiedPositions, 7f, 0f, Vector2.zero);
        int noSacs = Random.Range(3, 5);
        while (noSacs > 0)
        {
            float interval = Random.Range(0.1f, 0.2f);
            while (interval > 0f)
            {
                interval = -Time.deltaTime;
                yield return null;
            }

            Vector2 position = GetEmptySpaceNearby(occupiedPositions, 1f, 0.25f, emptyPosition);
            WormSacBehaviour sac = _wormSacs.Create();
            sac.Initialise(position);
            occupiedPositions.Add(sac.transform);
            --noSacs;
            yield return null;
        }

        _spawning = false;
        _timeToNextSac = Random.Range(5f, 10f);
    }

    public static void Unregister(WormBodyBehaviour wormBodyBehaviour)
    {
        _worms.Remove(wormBodyBehaviour);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void TakeShotDamage(Shot shot)
    {
        TakeDamage(shot.DamageDealt());
    }

    public void TakeRawDamage(float damage, Vector2 direction)
    {
        TakeDamage(damage);
    }

    public void TakeExplosionDamage(float damage, Vector2 origin)
    {
        TakeDamage(damage);
    }

    public void Decay()
    {
    }

    public void Burn()
    {
    }

    public void Sicken(int stacks = 0)
    {
    }

    public bool IsDead()
    {
        return _healthController.GetCurrentHealth() == 0;
    }

    public void Kill()
    {
    }

    public void MyUpdate()
    {
        TrySpawnWorm();
        TrySpawnSacs();
    }

    public static Boss Instance()
    {
        return _instance;
    }

    public static void TakeDamage(float damage)
    {
        if (_instance._healthController.GetCurrentHealth() == 0) return;
        _instance._healthController.TakeDamage(damage);
        if (_instance._healthController.GetCurrentHealth() != 0) return;
        _instance.KillBoss();
    }

    public static void ReturnSac(WormSacBehaviour wormSacBehaviour)
    {
        _instance._wormSacs.Return(wormSacBehaviour);
        _instance.UnregisterSection(wormSacBehaviour);
    }
}