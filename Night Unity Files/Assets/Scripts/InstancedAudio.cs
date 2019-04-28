using System.Collections;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using UnityEngine.Audio;

public class InstancedAudio : MonoBehaviour
{
    private AudioSource _audioSource;
    private AudioHighPassFilter _hpf;
    private AudioLowPassFilter _lpf;
    private ObjectPool<InstancedAudio> _audioPool;
    private float _hpfCutoff, _lpfCutoff;

    public void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _hpf = GetComponent<AudioHighPassFilter>();
        _lpf = GetComponent<AudioLowPassFilter>();
    }

    public void SetMixerGroup(ObjectPool<InstancedAudio> audioPool, AudioMixerGroup mixerGroup, float spatialBlend)
    {
        _audioPool = audioPool;
        _audioSource.outputAudioMixerGroup = mixerGroup;
        _audioSource.spatialBlend = spatialBlend;
        _hpfCutoff = 0f;
        _lpfCutoff = 25000;
    }

    public void Play(AudioClip clip)
    {
        Play(clip, 1);
    }

    public void Play(AudioClip clip, float volume)
    {
        Play(clip, volume, 1);
    }

    public void SetMinMaxDistance(float min, float max)
    {
        _audioSource.minDistance = min;
        _audioSource.maxDistance = max;
    }

    public void SetLowPassCutoff(float cutoff) => _lpfCutoff = cutoff;

    public void SetHighPassCutoff(float cutoff) => _hpfCutoff = cutoff;

    public void Play(AudioClip clip, float volume, float pitch)
    {
        _hpf.cutoffFrequency = _hpfCutoff;
        _lpf.cutoffFrequency = _lpfCutoff;
        _audioSource.pitch = pitch;
        _audioSource.volume = volume;
        PlayClip(clip);
    }

    private void PlayClip(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.Play();
        StartCoroutine(WaitToDie());
    }

    private IEnumerator WaitToDie()
    {
        while (_audioSource.isPlaying) yield return null;
        _audioPool.Return(this);
    }

    private void OnDestroy()
    {
        _audioPool?.Dispose(this);
    }
}