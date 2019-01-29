using System.Linq;
using DG.Tweening;
using Game.Combat.Enemies;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

public class CacheButtonController : MonoBehaviour
{
    private SpriteRenderer _icon, _border, _glow;
    private SpriteRenderer _spawnGlow;
    private ParticleSystem _gateParticles, _edgeParticles;
    private Tweener _spawnTween, _glowTween, _iconTween, _borderTween;
    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _icon = gameObject.FindChildWithName<SpriteRenderer>("Icon");
        _border = gameObject.FindChildWithName<SpriteRenderer>("Select");
        _glow = gameObject.FindChildWithName<SpriteRenderer>("Glow");
        GameObject gateObject = transform.parent.Find("Gate").gameObject;
        PolygonCollider2D collider = gateObject.GetComponent<PolygonCollider2D>();
        Polygon b = new Polygon(collider.points.ToList(), Vector2.zero);
        WorldGrid.AddBarrier(b);
        _spawnGlow = gateObject.FindChildWithName<SpriteRenderer>("Glow");
        _gateParticles = gateObject.FindChildWithName<ParticleSystem>("Particle System");
        _edgeParticles = transform.parent.FindChildWithName<ParticleSystem>("Edge");
    }

    public void SetEdgeActive(bool active)
    {
        if (active) _edgeParticles.Play();
        else _edgeParticles.Stop();
    }

    public void SetGateActive(bool active)
    {
        if (active) _gateParticles.Play();
        else _gateParticles.Stop();
    }

    public void SpawnInEnemy(EnemyType enemyType)
    {
        _spawnTween?.Kill();
        _spawnGlow.SetAlpha(0.75f);
        _spawnTween = _spawnGlow.DOFade(0f, 1f);
        Transform spawnTransform = _spawnGlow.transform;
        Vector2 spawnLocation = spawnTransform.position - spawnTransform.forward * 0.5f;
        CombatManager.SpawnEnemy(enemyType, spawnLocation);
    }

    private void KillTweens()
    {
        _iconTween?.Kill();
        _glowTween?.Kill();
        _borderTween?.Kill();
    }

    public void ActivateButton()
    {
        KillTweens();
        _icon.SetAlpha(1f);
        _glow.SetAlpha(1f);
        _border.SetAlpha(1f);
        _glowTween = _glow.DOFade(0.5f, 1f);
        _audioSource.clip = AudioClips.Chimes.RandomElement();
        _audioSource.Play();
    }

    public void DeactivateButton()
    {
        KillTweens();
        _glowTween = _glow.DOFade(0f, 1f);
        _iconTween = _icon.DOFade(0.5f, 1f);
        _borderTween = _border.DOFade(0.5f, 1f);
    }

    public void DisableButton()
    {
        KillTweens();
        _glowTween = _glow.DOFade(0f, 1f);
        _iconTween = _icon.DOFade(0.25f, 1f);
        _borderTween = _border.DOFade(0.25f, 1f);
        Destroy(GetComponent<CircleCollider2D>());
        Destroy(this);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        CacheController.Instance().TryActivateButton(this);
    }
}