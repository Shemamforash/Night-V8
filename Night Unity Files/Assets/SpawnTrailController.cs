using System.Collections;
using DG.Tweening;
using Fastlights;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

public class SpawnTrailController : MonoBehaviour
{
    private static ObjectPool<SpawnTrailController> _spawnPool = new ObjectPool<SpawnTrailController>("Spawn Trails", "Prefabs/Combat/Effects/Spawn Trail");
    private Vector2 _from, _to;
    private const float Speed = 3f;
    private float _currentTime;
    private ParticleSystem _explosionParticles;
    private ParticleSystem _trailParticles;
    private FastLight _fastLight;
    private EnemyType _typeToSpawn;

    public void Awake()
    {
        _explosionParticles = gameObject.FindChildWithName<ParticleSystem>("Explosion");
        _trailParticles = gameObject.FindChildWithName<ParticleSystem>("Trails");
        _fastLight = GetComponent<FastLight>();
    }

    public static void Create(Vector2 from, Vector2 to, EnemyType typeToSpawn)
    {
        SpawnTrailController spawnTrail = _spawnPool.Create();
        spawnTrail.Initialise(from, to, typeToSpawn);
    }

    private void Initialise(Vector2 @from, Vector2 to, EnemyType typeToSpawn)
    {
        _from = from;
        _to = to;
        _typeToSpawn = typeToSpawn;
        transform.rotation = Quaternion.Euler(0, 0, AdvancedMaths.AngleFromUp(_from, _to));
        transform.position = from;
        _fastLight.enabled = false;

        float distance = _from.Distance(_to);
        float duration = distance / Speed;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOMove(to, duration));
        sequence.AppendCallback(() => StartCoroutine(WaitForParticles()));
    }

    private IEnumerator WaitForParticles()
    {
        _explosionParticles.Emit(100);
        _trailParticles.Emit(150);
        _fastLight.enabled = true;

        CombatManager.SpawnEnemy(_typeToSpawn, transform.position);

        float timer = 1f;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            _fastLight.Radius = timer;
            yield return null;
        }

        _fastLight.enabled = false;
        while (_explosionParticles.particleCount != 0
               && _trailParticles.particleCount != 0) yield return null;
        _spawnPool.Return(this);
    }

    public void OnDestroy()
    {
        _spawnPool.Dispose(this);
    }
}