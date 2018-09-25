using System.Collections;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

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

    public void Play(ObjectPool<InstancedAudio> audioPool, AudioClip clip, float volumeOffset, float hpfValue)
    {
        _audioPool = audioPool;
        _hpf.cutoffFrequency = hpfValue;
        _audioSource.clip = clip;
        _audioSource.pitch = Random.Range(0.9f, 1.1f);
        _audioSource.volume = Random.Range(0.9f, 1.1f) + volumeOffset;
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