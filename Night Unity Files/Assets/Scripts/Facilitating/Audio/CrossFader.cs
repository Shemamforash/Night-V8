using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

public class CrossFader : MonoBehaviour
{
	private AudioSource _audioSourceA, _audioSourceB;
	private float       _crossFadeDuration = 2f;
	private AudioSource _currentAudioSource;
	private AudioClip   _currentClip;
	private bool        _initialised;
	private float       _maxVolume;
	private bool        _startAtRandom;

	private void Initialise()
	{
		if (_initialised) return;
		_audioSourceA                       = new GameObject().AddComponent<AudioSource>();
		_audioSourceB                       = new GameObject().AddComponent<AudioSource>();
		_audioSourceA.outputAudioMixerGroup = Resources.Load<AudioMixer>("AudioMixer/Master").FindMatchingGroups("Modified")[0];
		_audioSourceB.outputAudioMixerGroup = Resources.Load<AudioMixer>("AudioMixer/Master").FindMatchingGroups("Modified")[0];
		_audioSourceA.transform.SetParent(transform);
		_audioSourceB.transform.SetParent(transform);
		_initialised = true;
	}

	public void SetCrossFadeDuration(float duration)
	{
		_crossFadeDuration = duration;
	}

	public void StartAtRandomPosition()
	{
		_startAtRandom = true;
	}

	public void SetLooping()
	{
		Initialise();
		_audioSourceA.loop = true;
		_audioSourceB.loop = true;
	}

	public void SetMaxVolume(float maxVolume)
	{
		_maxVolume = Mathf.Clamp(maxVolume, 0f, 1f);
	}

	public void SetMixerGroup(string groupName)
	{
		Initialise();
		_audioSourceA.outputAudioMixerGroup = Resources.Load<AudioMixer>("AudioMixer/Master").FindMatchingGroups(groupName)[0];
		_audioSourceB.outputAudioMixerGroup = Resources.Load<AudioMixer>("AudioMixer/Master").FindMatchingGroups(groupName)[0];
	}

	public void CrossFade(AudioClip to)
	{
		Initialise();
		if (_currentAudioSource != null)
		{
			if (_currentAudioSource.clip == to) return;
			Sequence    sequence = DOTween.Sequence();
			AudioSource fadeOut  = _currentAudioSource;
			sequence.Append(fadeOut.DOFade(0, _crossFadeDuration));
			sequence.AppendCallback(() => fadeOut.Stop());
		}

		_currentAudioSource      = _audioSourceA == _currentAudioSource ? _audioSourceB : _audioSourceA;
		_currentClip             = to;
		_currentAudioSource.clip = _currentClip;
		if (_startAtRandom && _currentClip != null)
		{
			float randomPosition = Random.Range(0f, to.length);
			_currentAudioSource.time = randomPosition;
		}

		_currentAudioSource.volume = 0;
		_currentAudioSource.Play();
		_currentAudioSource.DOFade(_maxVolume, _crossFadeDuration);
	}

	public float TimeRemaining() => _currentClip.length - _currentAudioSource.time;
}