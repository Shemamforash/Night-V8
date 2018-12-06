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
    private const float LeadDuration = 1f;
    private const float BeamDuration = 3f;
    private ParticleSystem _burst, _charge, _energy, _spray, _spray1, _spray2;
    private Transform _origin;
    private Vector3 _targetPosition;
    private bool _firing;
    private const int BeamDamage = 5;
    private float _lastBeamDamage;

    public static BeamController Create(Transform origin)
    {
        BeamController beamController = _beamPool.Create();
        beamController.Initialise(origin);
        return beamController;
    }

    public void Awake()
    {
        _burst = gameObject.FindChildWithName<ParticleSystem>("Burst");
        _charge = gameObject.FindChildWithName<ParticleSystem>("Charge");
        _energy = gameObject.FindChildWithName<ParticleSystem>("Energy");
        _spray = gameObject.FindChildWithName<ParticleSystem>("Spray");
        _spray1 = gameObject.FindChildWithName<ParticleSystem>("Spray 1");
        _spray2 = gameObject.FindChildWithName<ParticleSystem>("Spray 2");
        _glowLine = gameObject.FindChildWithName<LineRenderer>("Glow");
        _beamLine = gameObject.FindChildWithName<LineRenderer>("Beam");
        _leadLine = gameObject.FindChildWithName<LineRenderer>("Lead");
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
        ShowLine();
    }

    private void SetLeadLineActive()
    {
        _leadLine.enabled = true;
        _glowLine.enabled = false;
        _beamLine.enabled = false;
    }

    private void SetBeamLineActive()
    {
        _leadLine.enabled = false;
        _glowLine.enabled = true;
        _beamLine.enabled = true;
    }

    private void SetNoLineActive()
    {
        _leadLine.enabled = false;
        _glowLine.enabled = false;
        _beamLine.enabled = false;
        _burst.Stop();
        _spray.Stop();
        _spray1.Stop();
        _spray2.Stop();
        _firing = false;
    }

    private void ShowLine()
    {
        _charge.Play();
        _energy.Play();
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(SetLeadLineActive);
        Color startColor = UiAppearanceController.InvisibleColour;
        Color endColor = new Color(1f, 1f, 1f, 0.5f);
        sequence.Append(_leadLine.DOColor(new Color2(startColor, startColor), new Color2(endColor, endColor), LeadDuration));
        sequence.AppendCallback(() =>
        {
            _firing = true;
            SetBeamLineActive();
            _burst.Clear();
            _spray.Clear();
            _spray1.Clear();
            _spray2.Clear();
            _burst.Play();
            _spray.Play();
            _spray1.Play();
            _spray2.Play();
        });

        sequence.AppendInterval(0.1f);
        startColor = Color.white;
        endColor = UiAppearanceController.InvisibleColour;
        sequence.Append(_beamLine.DOColor(new Color2(startColor, startColor), new Color2(endColor, endColor), BeamDuration).SetEase(Ease.OutExpo));
        sequence.Insert(LeadDuration + 0.5f, _glowLine.DOColor(new Color2(startColor, startColor), new Color2(endColor, endColor), BeamDuration).SetEase(Ease.OutExpo));
        sequence.AppendCallback(SetNoLineActive);
        sequence.AppendInterval(1f);
        sequence.AppendCallback(() => _beamPool.Return(this));
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

        _lastBeamDamage = 0.2f;
        ContactFilter2D cf = new ContactFilter2D();
        cf.layerMask = (1 << 17) | (1 << 8) | (1 << 14);
        Collider2D[] hits = new Collider2D[100];
        int count = gameObject.GetComponent<BoxCollider2D>().OverlapCollider(cf, hits);
        for (int i = 0; i < count; ++i)
        {
            Collider2D hit = hits[i];
            if (!hit.gameObject.CompareTag("Player")) continue;
            Vector2 dir = transform.up;
            int damage = Mathf.CeilToInt(BeamDamage + WorldState.NormalisedDifficulty() * BeamDamage);
            PlayerCombat.Instance.TakeRawDamage(damage, dir);
            PlayerCombat.Instance.MovementController.KnockBack(dir, 15f);
            break;
        }
    }
}