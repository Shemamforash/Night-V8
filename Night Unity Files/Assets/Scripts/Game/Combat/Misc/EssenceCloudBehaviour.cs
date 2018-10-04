using System.Collections;
using Game.Combat.Generation;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

public class EssenceCloudBehaviour : MonoBehaviour
{
    private ParticleSystem _essenceParticles;
    private ParticleSystem _essencePuff;
    private const int DecayRate = 1;
    private float _currentTime;
    private static readonly ObjectPool<EssenceCloudBehaviour> _essenceCloudPool = new ObjectPool<EssenceCloudBehaviour>("Essence Clouds", "Prefabs/Combat/Visuals/Essence Cloud");
    private int _essenceCount;

    public void Awake()
    {
        _essenceParticles = gameObject.FindChildWithName<ParticleSystem>("Cloud");
        _essencePuff = gameObject.FindChildWithName<ParticleSystem>("Essence Puff");
        Initialise(20, transform.position);
    }

    public static void Create(Vector2 position, int essenceCount)
    {
        EssenceCloudBehaviour essence = _essenceCloudPool.Create();
        essence.Initialise(essenceCount, position);
    }

    private void Initialise(int essenceCount, Vector2 position)
    {
        _essenceCount = essenceCount;
        transform.position = position;
        SetEmissionRate();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        _essencePuff.Emit(_essenceCount);
        _essenceCloudPool.Return(this);
        Inventory inventory = PlayerCombat.Instance.Player.Inventory();
        inventory.IncrementResource("Essence", _essenceCount);
    }

    public void OnDestroy()
    {
        _essenceCloudPool.Dispose(this);
    }

    private IEnumerator Fade()
    {
        _essenceParticles.Stop();
        while (_essenceParticles.particleCount > 0) yield return null;
        _essenceCloudPool.Return(this);
    }

    public void Update()
    {
        if (!CombatManager.IsCombatActive()) return;
        if (!_essenceParticles.isPlaying) return;
        _currentTime += Time.deltaTime;
        if (_currentTime < DecayRate) return;
        --_essenceCount;
        SetEmissionRate();
        _currentTime = 0f;
    }

    private void SetEmissionRate()
    {
        if (_essenceCount == 0)
        {
            StartCoroutine(Fade());
            return;
        }

        ParticleSystem.EmissionModule emission = _essenceParticles.emission;
        emission.rateOverTime = _essenceCount;
    }
}