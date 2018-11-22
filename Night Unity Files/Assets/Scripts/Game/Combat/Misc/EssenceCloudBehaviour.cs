using System.Collections;
using Game.Combat.Generation;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.BaseGameFunctionality.InventorySystem;
using SamsHelper.Libraries;
using UnityEngine;

public class EssenceCloudBehaviour : MonoBehaviour
{
    private ParticleSystem _essenceParticles;
    private ParticleSystem _essencePuff;
    private float _currentTime;
    private static readonly ObjectPool<EssenceCloudBehaviour> _essenceCloudPool = new ObjectPool<EssenceCloudBehaviour>("Essence Clouds", "Prefabs/Combat/Visuals/Essence Cloud");

    public void Awake()
    {
        _essenceParticles = gameObject.FindChildWithName<ParticleSystem>("Cloud");
        _essencePuff = gameObject.FindChildWithName<ParticleSystem>("Essence Puff");
        Initialise(transform.position);
    }

    public static void Create(Vector2 position)
    {
        EssenceCloudBehaviour essence = _essenceCloudPool.Create();
        essence.Initialise(position);
    }

    private void Initialise(Vector2 position)
    {
        transform.position = position;
        SetEmissionRate();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        _essencePuff.Emit(20);
        _essenceCloudPool.Return(this);
        Inventory.IncrementResource("Essence", 1);
//        StartCoroutine(Fade());
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

    private void SetEmissionRate()
    {
        ParticleSystem.EmissionModule emission = _essenceParticles.emission;
        emission.rateOverTime = 20;
    }
}