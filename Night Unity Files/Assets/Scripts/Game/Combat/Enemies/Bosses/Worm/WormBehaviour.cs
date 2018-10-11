using System.Collections;
using System.Collections.Generic;
using Game.Combat.Enemies.Bosses;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;

public class WormBehaviour : Boss
{
    private float _timeToNextWorm, _timeToNextSac;
    private static GameObject _prefab;
    private readonly HealthController _healthController = new HealthController();
    private bool _spawning;
    private List<WormSacBehaviour> _wormSacs = new List<WormSacBehaviour>();

    protected override void Awake()
    {
        base.Awake();
        _healthController.SetInitialHealth(2000, null);
    }

    public static void Create()
    {
        Instantiate(Resources.Load<GameObject>("Prefabs/Combat/Bosses/Worm/Worm"));
    }

    public override void UnregisterSection(BossSectionHealthController section)
    {
        Sections.Remove(section);
        _wormSacs.Remove((WormSacBehaviour) section);
    }

    public void Update()
    {
        if (!CombatManager.IsCombatActive()) return;
        MyUpdate();
    }

    private void TrySpawnWorm()
    {
        _timeToNextWorm -= Time.deltaTime;
        if (_timeToNextWorm > 0f) return;
        Vector2 wormPosition = PlayerCombat.Instance.transform.position;
        if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Bosses/Worm/Worm Body");
        WormBodyBehaviour newWorm = Instantiate(_prefab).GetComponent<WormBodyBehaviour>();
        newWorm.Initialise(wormPosition);
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
        _wormSacs.RemoveAll(w => w == null);
        _wormSacs.ForEach(sac => occupiedPositions.Add(sac.transform));

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
            WormSacBehaviour sac = WormSacBehaviour.Create(this);
            _wormSacs.Add(sac);
            sac.Initialise(position);
            occupiedPositions.Add(sac.transform);
            --noSacs;
            yield return null;
        }

        _spawning = false;
        _timeToNextSac = Random.Range(5f, 10f);
    }

    public void MyUpdate()
    {
        TrySpawnWorm();
        TrySpawnSacs();
    }

    public void TakeDamage(int damage)
    {
        _healthController.TakeDamage(damage);
        if (_healthController.GetCurrentHealth() != 0) return;
        Kill();
    }

    public int CurrentHealth()
    {
        return (int) _healthController.GetCurrentHealth();
    }

    public int MaxHealth()
    {
        return (int) _healthController.GetMaxHealth();
    }
}