using System.Collections;
using SamsHelper.Libraries;
using UnityEngine;

public class EndGameAudioController : MonoBehaviour
{
    private const float Duration = 30f;
    private AudioSource _boom, _beep;
    private float _beepVolume, _boomVolume, _timeToNextBeep;
    private float _normalisedTime;

    public void Awake()
    {
        _boom = gameObject.FindChildWithName<AudioSource>("Boom");
        _beep = gameObject.FindChildWithName<AudioSource>("Beep");
        StartCoroutine(StartBeep());
    }

    private float GetBoomVolume()
    {
        if (_normalisedTime < 0.6f) return _normalisedTime / 0.6f;
        if (_normalisedTime < 0.8f) return 1f;
        return (1 - _normalisedTime) / 0.2f;
    }

    private float GetBeepVolume()
    {
        if (_normalisedTime < 0.6f) return 0f;
        return 1 - (1 - _normalisedTime) / 0.4f;
    }

    private IEnumerator StartBeep()
    {
        float elapsedTime = 0f;
        _timeToNextBeep = -1;

        while (elapsedTime < Duration)
        {
            elapsedTime += Time.deltaTime;
            _normalisedTime = elapsedTime / Duration;
            _boomVolume = GetBoomVolume();
            _beepVolume = GetBeepVolume();
            Beep();
            yield return null;
        }
    }

    private void Beep()
    {
        _timeToNextBeep -= Time.deltaTime;
        if (_timeToNextBeep > 0) return;
        _timeToNextBeep = 1;
        _boom.volume = _boomVolume;
        _beep.volume = _beepVolume;
        _boom.Play();
        _beep.Play();
    }
}