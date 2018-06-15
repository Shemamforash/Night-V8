using System.Collections;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

public class EssenceCloudBehaviour : MonoBehaviour
{
    private ParticleSystem _essenceParticles;
    private ParticleSystem _essencePuff;
    private const int DecayRate = 1;
    private float _currentTime;
    private static readonly ObjectPool<EssenceCloudBehaviour> _essenceCloudPool = new ObjectPool<EssenceCloudBehaviour>("Prefabs/Combat/Essence Cloud");
    private int _essenceCount;

    public void Awake()
    {
        _essenceParticles = Helper.FindChildWithName<ParticleSystem>(gameObject, "Cloud");
        _essencePuff = Helper.FindChildWithName<ParticleSystem>(gameObject, "Essence Puff");
        Initialise(20, transform.position);
    }

    public static void Create(int essenceCount, Vector2 position)
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
    }

    public void OnDestroy()
    {
        _essenceCloudPool.Dispose(this);
    }

    private IEnumerator Fade()
    {
        _essenceParticles.Stop();
        while (_essenceParticles.particleCount > 0) yield return null;
    }

    public void Update()
    {
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