using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class HeartBeatController : MonoBehaviour
{
    [Range(0f, 1f)] public float Health;

    private ParticleSystem _heartBeatParticles;
    private float _heartBeatLongGap = 1f;
    private float _heartBeatShortGap = 0.25f;
    private float _currentTime;
    private bool _timingLongBeat;
    private static HeartBeatController _instance;

    public void Awake()
    {
        _instance = this;
        _heartBeatParticles = GetComponent<ParticleSystem>();
    }

    public void Start()
    {
        SetTrailAlpha(0f);
    }

    public static void SetHealth(float health)
    {
        _instance.Health = health;
        if (_instance.Health <= 0.4f)
        {
            _instance._heartBeatParticles.Play();
        }
        else
        {
            _instance._heartBeatParticles.Stop();
        }
    }

    public void Update()
    {
        if (Health >= 0.4f) return;
        float maxAlpha = 1f - Health / 0.4f;
        float minAlpha = Mathf.Clamp(maxAlpha - 0.6f, 0f, 1f);
        float currentAlpha;
        _currentTime += Time.deltaTime;
        if (_timingLongBeat)
        {
            if (_currentTime >= _heartBeatLongGap)
            {
                _timingLongBeat = false;
                _currentTime -= _heartBeatLongGap;
            }

            currentAlpha = 1f - _currentTime / _heartBeatLongGap;
        }
        else
        {
            if (_currentTime >= _heartBeatShortGap)
            {
                _timingLongBeat = true;
                _currentTime -= _heartBeatShortGap;
            }

            currentAlpha = 1f - _currentTime / _heartBeatShortGap;
        }

        if (currentAlpha < 0)
        {
            currentAlpha = 0f;
        }
        currentAlpha *= maxAlpha - minAlpha;
        currentAlpha += minAlpha;
        SetTrailAlpha(currentAlpha);
    }

    private void SetTrailAlpha(float alpha)
    {
        ParticleSystem.TrailModule trails = _heartBeatParticles.trails;
        trails.colorOverTrail = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, alpha), UiAppearanceController.InvisibleColour);
    }
}