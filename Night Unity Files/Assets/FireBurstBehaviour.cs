using DG.Tweening;
using Fastlights;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using UnityEngine;

public class FireBurstBehaviour : FireDamageDeal
{
    private static readonly ObjectPool<FireBurstBehaviour> _firePool = new ObjectPool<FireBurstBehaviour>("Fire Areas", "Prefabs/Combat/Effects/Fire Burst");
    private SpriteRenderer _smallFlash, _flash;
    private ParticleSystem _fire, _swirl, _burst;
    private FastLight _light;
    private const float LifeTime = 8f;

    private void Awake()
    {
        _smallFlash = gameObject.FindChildWithName<SpriteRenderer>("Small Flash");
        _flash = gameObject.FindChildWithName<SpriteRenderer>("Flash");
        _fire = GetComponent<ParticleSystem>();
        _swirl = gameObject.FindChildWithName<ParticleSystem>("Swirl");
        _burst = gameObject.FindChildWithName<ParticleSystem>("Burst");
        _light = gameObject.FindChildWithName<FastLight>("Light");
    }

    public static FireBurstBehaviour Create(Vector3 position)
    {
        FireBurstBehaviour burst = _firePool.Create();
        burst.Initialise(position);
        return burst;
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

    private void RestartParticles()
    {
        _swirl.Clear();
        _burst.Clear();
        _fire.Clear();
        _swirl.Play();
        _burst.Play();
        _fire.Play();
        _light.SetAlpha(1f);
    }

    private void Burst()
    {
        _smallFlash.SetAlpha(0f);
        _flash.SetAlpha(0f);
        _light.SetAlpha(0f);
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(() => _smallFlash.SetAlpha(1));
        sequence.AppendInterval(0.1f);
        sequence.AppendCallback(() =>
        {
            _smallFlash.SetAlpha(0f);
            _flash.SetAlpha(1f);
        });
        sequence.AppendCallback(RestartParticles);
        sequence.Append(_flash.DOFade(0f, 1f));
        sequence.AppendInterval(LifeTime - 1f);
        sequence.AppendCallback(() => _fire.Stop());
        sequence.Append(DOTween.To(_light.GetAlpha, _light.SetAlpha, 0f, 1f));
        sequence.AppendCallback(() => _firePool.Return(this));
    }
}