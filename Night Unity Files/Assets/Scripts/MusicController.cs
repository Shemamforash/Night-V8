using DG.Tweening;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicController : MonoBehaviour
{
    private AudioSource _audioSource;

    public void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        PlayClip();
    }

    public void PlayClip()
    {
        _audioSource.clip = AudioClips.Ambient.RandomElement();
        _audioSource.time = Random.Range(0f, _audioSource.clip.length);
        Sequence sequence = DOTween.Sequence();
        _audioSource.volume = 0f;
        _audioSource.Play();
        sequence.Append(_audioSource.DOFade(1f, 2f));
        sequence.AppendInterval(_audioSource.clip.length - 2f);
        sequence.Append(_audioSource.DOFade(0f, 2f));
        sequence.AppendCallback(PlayClip);
    }
}