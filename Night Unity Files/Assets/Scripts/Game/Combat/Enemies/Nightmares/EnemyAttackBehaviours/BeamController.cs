using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class BeamController : MonoBehaviour
{
    private static readonly ObjectPool<BeamController> _beamPool = new ObjectPool<BeamController>("Beams", "Prefabs/Combat/Enemies/Beam");
    private static GameObject _prefab;
    private LineRenderer _glowLine, _beamLine, _leadLine;
    private const float LeadDuration = 0.5f;
    private const float BeamDuration = 2f;
    private Transform _origin;
    private Vector3 _targetPosition;
    private bool _firing;
    private const int BeamDamage = 15;
    private float _lastBeamDamage;

    private ParticleSystem[] _blastParticles, _chargeParticles;
    private AudioSource[] _audioSources;
    private GameObject _blastObject, _lineObject;
    private AudioSource _chargeAudio;
    private bool _stopFiring;
    private bool _beamActive;

    public void Awake()
    {
        _blastObject = gameObject.FindChildWithName("Blast");
        _blastParticles = _blastObject.GetComponentsInChildren<ParticleSystem>();
        _chargeParticles = gameObject.FindChildWithName("Charge Up").GetComponentsInChildren<ParticleSystem>();
        _chargeAudio = gameObject.FindChildWithName<AudioSource>("Audio");
        _audioSources = _chargeAudio.gameObject.GetComponentsInChildren<AudioSource>();


        _lineObject = gameObject.FindChildWithName("Lines");
        _glowLine = gameObject.FindChildWithName<LineRenderer>("Glow");
        _beamLine = gameObject.FindChildWithName<LineRenderer>("Beam");
        _leadLine = gameObject.FindChildWithName<LineRenderer>("Lead");
    }

    public static BeamController Create(Transform origin)
    {
        BeamController beamController = _beamPool.Create();
        beamController.Initialise(origin);
        return beamController;
    }

    private void LateUpdate()
    {
        if (_origin == null) return;
        if (_firing) DoBeamDamage();
        transform.position = _origin.position;
        _targetPosition = transform.position + _origin.up * 50f;
        Vector3[] positions = {transform.position, _targetPosition};
        _glowLine.SetPositions(positions);
        _beamLine.SetPositions(positions);
        _leadLine.SetPositions(positions);
        float rotation = AdvancedMaths.AngleFromUp(transform.position, _targetPosition) + 90;
        transform.rotation = Quaternion.Euler(0, 0, rotation);
    }

    private void Initialise(Transform origin)
    {
        _origin = origin;
        transform.position = _origin.position;
        StartCoroutine(WarmUp());
    }

    private IEnumerator WarmUp()
    {
        _chargeAudio.Play();
        Array.ForEach(_chargeParticles, p => p.Play());
        _lineObject.SetActive(true);
        _leadLine.enabled = true;
        _glowLine.enabled = false;
        _beamLine.enabled = false;
        Color startColor = UiAppearanceController.InvisibleColour;
        Color endColor = new Color(1f, 1f, 1f, 0.5f);
        _leadLine.DOColor(new Color2(startColor, startColor), new Color2(endColor, endColor), LeadDuration);
        yield return DOTween.To(() => _leadLine.startWidth, f =>
        {
            _leadLine.startWidth = f;
            _leadLine.endWidth = f;
        }, 0.05f, LeadDuration).WaitForCompletion();
        yield return StartCoroutine(FireBeam());
    }

    private IEnumerator FireBeam()
    {
        Array.ForEach(_audioSources, s =>
        {
            s.volume = 0.5f;
            s.Play();
        });
        _beamActive = true;
        _firing = true;
        _blastObject.SetActive(true);
        Array.ForEach(_chargeParticles, p => p.Stop());
        Array.ForEach(_blastParticles, b => b.Play());
        _beamLine.enabled = true;
        _glowLine.enabled = true;
        _leadLine.enabled = false;
        float remainingTime = BeamDuration;
        while (remainingTime > 0f)
        {
            if (_stopFiring) break;
            float noise = Mathf.PerlinNoise(Time.timeSinceLevelLoad, 0f) / 2f + 0.5f;
            float glowWidth = 0.5f * noise;
            float beamWidth = 0.1f * noise;
            _glowLine.startWidth = glowWidth;
            _glowLine.endWidth = glowWidth;
            _beamLine.startWidth = beamWidth;
            _beamLine.endWidth = beamWidth;
            remainingTime -= Time.deltaTime;
            yield return null;
        }

        _stopFiring = false;
        Array.ForEach(_audioSources, s => s.DOFade(0f, 0.5f));
        _lineObject.SetActive(false);
        Array.ForEach(_blastParticles, p => p.Stop());
        _firing = false;
        while (_blastParticles.Any(p => p.particleCount > 0)) yield return null;
        _beamActive = false;
        _blastObject.SetActive(false);
        _beamPool.Return(this);
    }

    private void OnDestroy()
    {
        _beamPool.Dispose(this);
    }

    private void DoBeamDamage()
    {
        if (_lastBeamDamage > 0f)
        {
            _lastBeamDamage -= Time.deltaTime;
            return;
        }

        ContactFilter2D cf = new ContactFilter2D();
        cf.layerMask = (1 << 17) | (1 << 8) | (1 << 14);
        Collider2D[] hits = new Collider2D[100];
        int count = gameObject.GetComponent<BoxCollider2D>().OverlapCollider(cf, hits);
        int damage = WorldState.ScaleValue(BeamDamage);
        for (int i = 0; i < count; ++i)
        {
            Collider2D hit = hits[i];
            if (!hit.gameObject.CompareTag("Player")) continue;
            Vector2 dir = transform.up;
            PlayerCombat.Instance.TakeRawDamage(damage, dir);
            _lastBeamDamage = 0.2f;
            PlayerCombat.Instance.MovementController.KnockBack(dir, 15f);
            break;
        }
    }

    public bool Active() => _beamActive;
}