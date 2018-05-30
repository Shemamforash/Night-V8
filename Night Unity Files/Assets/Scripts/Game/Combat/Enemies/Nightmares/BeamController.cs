using System.Collections;
using SamsHelper.Libraries;
using UnityEngine;

public class BeamController : MonoBehaviour
{
    private static GameObject _prefab;
    private LineRenderer _glowLine, _beamLine, _leadLine;
    private float LeadDurationMax;
    private float BeamDurationMax;
    private ParticleSystem _particleBurst;
    private Transform _origin, _target;
    private Vector2 _originPos, _targetPos;
    private bool _showParticles;
    private bool _followTransforms;
    private float _initialBeamWidth;
    
    public static BeamController Create(bool showParticles = true, float leadLineDuration = 1f, float beamDuration = 2f)
    {
        if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Enemies/Beam");
        GameObject beam = Instantiate(_prefab);
        BeamController beamController = beam.GetComponent<BeamController>();
        beamController.LeadDurationMax = leadLineDuration;
        beamController.BeamDurationMax = beamDuration;
        beamController._showParticles = showParticles;
        return beamController;
    }

    public void SetBeamWidth(float widthModifier)
    {
        float glowLineWidth = _glowLine.startWidth * widthModifier;
        float beamLineWidth = _beamLine.startWidth * widthModifier;
        float leadLineWidth = _leadLine.startWidth * widthModifier;
        _initialBeamWidth = beamLineWidth;
        
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0, glowLineWidth);
        curve.AddKey(1, glowLineWidth);
        _glowLine.widthCurve = curve;

        curve = new AnimationCurve();
        curve.AddKey(0, beamLineWidth);
        curve.AddKey(1, beamLineWidth);
        _beamLine.widthCurve = curve;

        curve = new AnimationCurve();
        curve.AddKey(0, leadLineWidth);
        curve.AddKey(1, leadLineWidth);
        _leadLine.widthCurve = curve;
    }

    private void SetPositions(Vector2 originPos, Vector2 targetPos)
    {
        _originPos = originPos;
        _targetPos = targetPos;
        Initialise();
    }

    public void SetFollowTransforms(Transform origin, Transform target)
    {
        _origin = origin;
        _target = target;
        _followTransforms = true;
        SetPositions(origin.position, target.position);
    }

    private void UpdatePosition()
    {
        Vector3[] positions = {_originPos, _targetPos};
        _glowLine.SetPositions(positions);
        _beamLine.SetPositions(positions);
        _leadLine.SetPositions(positions);
    }
    
    private void Initialise()
    {
        _glowLine = Helper.FindChildWithName<LineRenderer>(gameObject, "Glow");
        _beamLine = Helper.FindChildWithName<LineRenderer>(gameObject, "Beam");
        _leadLine = Helper.FindChildWithName<LineRenderer>(gameObject, "Lead");
        transform.position = _originPos;
        UpdatePosition();
        StartCoroutine(ShowLeadLine());
        if (!_showParticles) return;
        Vector2 direction = (_targetPos - _originPos).normalized;
        _particleBurst = Helper.FindChildWithName<ParticleSystem>(gameObject, "Burst");
        _particleBurst.transform.position = _originPos;
        _particleBurst.transform.rotation = Quaternion.Euler(new Vector3(0, 0, AdvancedMaths.AngleFromUp(Vector3.zero, direction)));
    }

    public void Update()
    {
        if(!_followTransforms) return;
        if(_origin != null) _originPos = _origin.position;
        if(_target != null) _targetPos = _target.position;
        UpdatePosition();
    }

    private IEnumerator ShowLeadLine()
    {
        float leadDuration = LeadDurationMax;
        _leadLine.enabled = true;
        _glowLine.enabled = false;
        _beamLine.enabled = false;
        while (leadDuration > 0f)
        {
            float normalisedTime = 1f - leadDuration / LeadDurationMax;
            Color leadColour = new Color(1, 1, 1, 0.2f + 0.4f * normalisedTime);
            _leadLine.startColor = leadColour;
            _leadLine.endColor = leadColour;
            leadDuration -= Time.deltaTime;
            yield return null;
        }

        _leadLine.enabled = false;
        StartCoroutine(ShowBeam());
    }

    private IEnumerator ShowBeam()
    {
        _glowLine.enabled = true;
        _beamLine.enabled = true;

        Color lineColor = new Color(1, 1, 1, 0.5f);
        _glowLine.startColor = lineColor;
        _glowLine.endColor = lineColor;
        _beamLine.startColor = new Color(1, 1, 1, 1);
        _beamLine.endColor = lineColor;
        
        float beamDuration = BeamDurationMax / 4f;
        while (beamDuration > 0f)
        {
            beamDuration -= Time.deltaTime;
            yield return null;
        }

        if(_showParticles) _particleBurst.Emit(25);
        beamDuration = BeamDurationMax;
        while (beamDuration > 0f)
        {
            beamDuration -= Time.deltaTime;
            float normalisedTime = beamDuration / BeamDurationMax;
            Color glowLineColor = new Color(1, 1, 1, normalisedTime * 0.5f);
            _glowLine.startColor = glowLineColor;
            _glowLine.endColor = glowLineColor;

            _beamLine.startColor = new Color(1, 1, 1, normalisedTime);
            _beamLine.endColor = new Color(1, 1, 1, normalisedTime * 0.5f);

            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, _initialBeamWidth * normalisedTime);
            curve.AddKey(1, _initialBeamWidth * normalisedTime);
            _beamLine.widthCurve = curve;
            
            yield return null;
        }

        _glowLine.enabled = false;
        _beamLine.enabled = false;
        Destroy(gameObject);
    }
}