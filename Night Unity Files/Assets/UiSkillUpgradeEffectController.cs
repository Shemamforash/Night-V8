using DG.Tweening;
using SamsHelper.Libraries;
using UnityEngine;

public class UiSkillUpgradeEffectController : MonoBehaviour
{
    private static ParticleSystem _pulse, _particles;
    private static SpriteRenderer _glow;

    public void Awake()
    {
        _pulse = gameObject.FindChildWithName<ParticleSystem>("Pulse");
        _particles = GetComponent<ParticleSystem>();
        _glow = gameObject.FindChildWithName<SpriteRenderer>("Glow");
    }

    public void Activate()
    {
        _pulse.Play();
        _particles.Play();
        _glow.SetAlpha(1f);
        _glow.DOFade(0f, 2f).SetUpdate(UpdateType.Normal, true);
    }
}