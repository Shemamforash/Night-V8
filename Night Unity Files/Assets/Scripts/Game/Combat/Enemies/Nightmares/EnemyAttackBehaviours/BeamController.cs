using DG.Tweening;
using Game.Combat.Player;
using SamsHelper.BaseGameFunctionality.Basic;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class BeamController : MonoBehaviour
{
    private static readonly ObjectPool<BeamController> _beamPool = new ObjectPool<BeamController>("Beams", "Prefabs/Combat/Enemies/Beam");
    private static GameObject _prefab;
    private LineRenderer _glowLine, _beamLine, _leadLine;
    private const float LeadDurationMax = 0.5f;
    private const float BeamDurationMax = 0.5f;
    private ParticleSystem _particleBurst;
    private Transform _origin;
    private Vector3 _targetPosition;
    private const int BeamDamage = 25;

    public static BeamController Create(Transform origin)
    {
        BeamController beamController = _beamPool.Create();
        beamController.Initialise(origin);
        return beamController;
    }

    public void Awake()
    {
        _particleBurst = gameObject.FindChildWithName<ParticleSystem>("Burst");
        _glowLine = gameObject.FindChildWithName<LineRenderer>("Glow");
        _beamLine = gameObject.FindChildWithName<LineRenderer>("Beam");
        _leadLine = gameObject.FindChildWithName<LineRenderer>("Lead");
    }

    private void LateUpdate()
    {
        if (_origin == null) return;
        transform.position = _origin.position;
        _targetPosition = transform.position + _origin.up * 20f;
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
    }

    private void ShowLine()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.AppendCallback(SetLeadLineActive);
        Color startColor = UiAppearanceController.InvisibleColour;
        Color endColor = new Color(1f, 1f, 1f, 0.5f);
        sequence.Append(_leadLine.DOColor(new Color2(startColor, startColor), new Color2(endColor, endColor), LeadDurationMax));
        sequence.AppendCallback(() =>
        {
            SetBeamLineActive();
            _particleBurst.Clear();
            _particleBurst.Play();
            DoBeamDamage();
        });
        startColor = Color.white;
        endColor = UiAppearanceController.InvisibleColour;
        sequence.Append(_beamLine.DOColor(new Color2(startColor, startColor), new Color2(endColor, endColor), BeamDurationMax).SetEase(Ease.OutExpo));
        sequence.Insert(LeadDurationMax, _glowLine.DOColor(new Color2(startColor, startColor), new Color2(endColor, endColor), BeamDurationMax).SetEase(Ease.OutExpo));
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
        ContactFilter2D cf = new ContactFilter2D();
        cf.layerMask = (1 << 17) | (1 << 8) | (1 << 14);
        Collider2D[] hits = new Collider2D[100];
        int count = gameObject.GetComponent<BoxCollider2D>().OverlapCollider(cf, hits);
        for (int i = 0; i < count; ++i)
        {
            Collider2D hit = hits[i];
            if (!hit.gameObject.CompareTag("Player")) continue;
            Vector2 dir = transform.up;
            PlayerCombat.Instance.TakeRawDamage(BeamDamage, dir);
            break;
        }
    }
}