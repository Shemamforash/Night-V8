using System.Collections;
using SamsHelper.Libraries;
using UnityEngine;

public class BeamController : MonoBehaviour
{
    private static GameObject _prefab;
    private LineRenderer _glowLine, _beamLine, _leadLine;
    private const float LeadLineDurationMax = 1f;
    private const float BeamDurationMax = 2f;
    private ParticleSystem _particleBurst;

    public static void Create(Vector2 origin, Vector2 direction)
    {
        direction.Normalize();
        if (_prefab == null) _prefab = Resources.Load<GameObject>("Prefabs/Combat/Enemies/Beam");
        GameObject beam = Instantiate(_prefab);
        beam.GetComponent<BeamController>().Initialise(origin, direction);
    }

    private void Initialise(Vector2 origin, Vector2 direction)
    {
        transform.position = origin;
        _glowLine = Helper.FindChildWithName<LineRenderer>(gameObject, "Glow");
        _beamLine = Helper.FindChildWithName<LineRenderer>(gameObject, "Beam");
        _leadLine = Helper.FindChildWithName<LineRenderer>(gameObject, "Lead");
        Vector3[] positions = {origin, origin + direction.normalized * 30};
        _glowLine.SetPositions(positions);
        _beamLine.SetPositions(positions);
        _leadLine.SetPositions(positions);
        _particleBurst = Helper.FindChildWithName<ParticleSystem>(gameObject, "Burst");
        _particleBurst.transform.position = origin;
        _particleBurst.transform.rotation = Quaternion.Euler(new Vector3(0, 0, AdvancedMaths.AngleFromUp(Vector3.zero, direction)));
        StartCoroutine(ShowLeadLine());
    }

    private IEnumerator ShowLeadLine()
    {
        float leadDuration = LeadLineDurationMax;
        _leadLine.enabled = true;
        _glowLine.enabled = false;
        _beamLine.enabled = false;
        while (leadDuration > 0f)
        {
            float normalisedTime = 1f - leadDuration / LeadLineDurationMax;
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
        
        float beamDuration = 0.5f;
        while (beamDuration > 0f)
        {
            beamDuration -= Time.deltaTime;
            yield return null;
        }

        _particleBurst.Emit(25);
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
            _beamLine.startWidth = 0.1f * normalisedTime;
            _beamLine.endWidth = 0.1f * normalisedTime;
            yield return null;
        }

        _glowLine.enabled = false;
        _beamLine.enabled = false;
        Destroy(gameObject);
    }
}