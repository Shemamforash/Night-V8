using DG.Tweening;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

public class TrailFireBehaviour : FireDamageDeal
{
    private static readonly ObjectPool<TrailFireBehaviour> _firePool = new ObjectPool<TrailFireBehaviour>("Fire Areas", "Prefabs/Combat/Effects/Fire Trail");
    private ParticleSystem _fire;
    private const float LifeTime = 4f;

    private void Awake()
    {
        _fire = GetComponent<ParticleSystem>();
    }

    public static TrailFireBehaviour Create(Vector3 position)
    {
        TrailFireBehaviour trailFire = _firePool.Create();
        trailFire.Initialise(position);
        return trailFire;
    }

    private void Initialise(Vector3 position)
    {
        Clear();
        transform.position = position;
        Burst();
    }

    public void OnDestroy()
    {
        _firePool.Dispose(this);
    }

    private void Burst()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(LifeTime);
        sequence.AppendCallback(() => _fire.Stop());
        sequence.AppendInterval(1f);
        sequence.AppendCallback(() => _firePool.Return(this));
    }
}