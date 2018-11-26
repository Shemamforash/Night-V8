using System.Collections;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using UnityEngine.Audio;

public class InstancedAudio : MonoBehaviour
{
    private AudioSource _audioSource;
    private AudioHighPassFilter _hpf;
    private ObjectPool<InstancedAudio> _audioPool;

    public void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _hpf = GetComponent<AudioHighPassFilter>();
    }

    public void SetMixerGroup(ObjectPool<InstancedAudio> audioPool, AudioMixerGroup mixerGroup, float spatialBlend)
    {
        _audioPool = audioPool;
        _audioSource.outputAudioMixerGroup = mixerGroup;
        _audioSource.spatialBlend = spatialBlend;
    }

    public void Play(AudioClip clip)
    {
        Play(clip, 1);
    }

    public void Play(AudioClip clip, float volume)
    {
        Play(clip, volume, 1);
    }

    public void Play(AudioClip clip, float volume, float pitch, float hpfCutoff = 0f)
    {
        _hpf.cutoffFrequency = hpfCutoff;
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
        _audioPool.Dispose(this);
    }
}