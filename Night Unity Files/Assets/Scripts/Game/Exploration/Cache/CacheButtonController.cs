using System.Collections.Generic;
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
    private ParticleSystem _gateParticles;
    private Tweener _spawnTween, _glowTween, _iconTween, _borderTween;
    private AudioSource _audioSource;
    private AudioSource _gateAudio;
    private bool _disabled;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _icon = gameObject.FindChildWithName<SpriteRenderer>("Icon");
        _border = gameObject.FindChildWithName<SpriteRenderer>("Select");
        _glow = gameObject.FindChildWithName<SpriteRenderer>("Glow");
        GameObject gateObject = transform.parent.Find("Gate").gameObject;
        _gateAudio = gateObject.GetComponent<AudioSource>();
        _gateAudio.volume = 0;
        PolygonCollider2D collider = gateObject.GetComponent<PolygonCollider2D>();
        List<Vector2> points = collider.points.ToList();
        float rotation = collider.transform.rotation.eulerAngles.z;
        for (int i = 0; i < points.Count; ++i)
        {
            Vector2 position = points[i];
            position = AdvancedMaths.RotatePoint(position, rotation, Vector2.zero, false);
            position += (Vector2) collider.transform.position;
            points[i] = position;
        }

        Polygon b = new Polygon(points, Vector2.zero);
        WorldGrid.AddBarrier(b);
        _spawnGlow = gateObject.FindChildWithName<SpriteRenderer>("Glow");
        _gateParticles = gateObject.FindChildWithName<ParticleSystem>("Particle System");
        DeactivateButton();
    }

    public void SetGateActive(bool active)
    {
        if (active)
        {
            _gateParticles.Play();
            _gateAudio.DOFade(1f, 2f);
        }
        else
        {
            _gateParticles.Stop();
            _gateAudio.DOFade(0f, 2f);
        }
    }

    public void SpawnInEnemy(EnemyType enemyType)
    {
        _spawnTween?.Kill();
        _spawnGlow.SetAlpha(1f);
        _spawnTween = _spawnGlow.DOFade(0f, 1f);
        Transform spawnTransform = _spawnGlow.transform;
        Vector2 spawnLocation = spawnTransform.position - spawnTransform.forward * 0.1f;
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
        if (_disabled) return;
        _disabled = true;
        KillTweens();
        _glowTween = _glow.DOFade(0f, 1f);
        _iconTween = _icon.DOFade(0f, 1f);
        _borderTween = _border.DOFade(0f, 1f);
        Destroy(GetComponent<CircleCollider2D>());
        Destroy(this);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (_disabled) return;
        CacheController.Instance().TryActivateButton(this);
    }
}