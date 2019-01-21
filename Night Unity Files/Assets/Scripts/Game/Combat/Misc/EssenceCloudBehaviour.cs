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
    private ParticleSystem _essenceCloud, _essenceRing;
    private ParticleSystem _essencePuff;
    private float _currentTime;
    private static readonly ObjectPool<EssenceCloudBehaviour> _essenceCloudPool = new ObjectPool<EssenceCloudBehaviour>("Essence Clouds", "Prefabs/Combat/Visuals/Essence Cloud");
    private bool _triggered;

    public void Awake()
    {
        _essenceCloud = gameObject.FindChildWithName<ParticleSystem>("Cloud");
        _essenceRing = gameObject.FindChildWithName<ParticleSystem>("Ring");
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
        _triggered = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.gameObject.CompareTag("Player")) return;
        _triggered = true;
        Inventory.IncrementResource("Essence", 1);
        StartCoroutine(Fade());
    }

    public void OnDestroy()
    {
        _essenceCloudPool.Dispose(this);
    }

    private IEnumerator Fade()
    {
        _essenceCloud.Stop();
        _essenceCloud.Clear();
        _essenceRing.Stop();
        _essenceRing.Clear();
        _essencePuff.Emit(30);
        while (_essencePuff.particleCount > 0) yield return null;
        _essenceCloudPool.Return(this);
    }
}