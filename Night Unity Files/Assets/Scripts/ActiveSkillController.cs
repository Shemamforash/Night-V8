using DG.Tweening;
using Game.Global;
using Extensions;
using UnityEngine;

public class ActiveSkillController : MonoBehaviour
{
	private static ActiveSkillController _instance;
	private static AudioSource           _audioSource;
	private static Tweener               _audioTweener;
	private        ParticleSystem        _swirl1, _swirl2, _swirl3;

	private void Awake()
	{
		_swirl1             = gameObject.FindChildWithName<ParticleSystem>("Swirl 1");
		_swirl2             = gameObject.FindChildWithName<ParticleSystem>("Swirl 2");
		_swirl3             = gameObject.FindChildWithName<ParticleSystem>("Swirl 3");
		_audioSource        = GetComponent<AudioSource>();
		_audioSource.clip   = AudioClips.MagazineEffect;
		_audioSource.volume = 0f;
		_audioSource.Play();
		_instance = this;
	}

	public static void Play()
	{
		_instance._swirl1.Play();
		_instance._swirl2.Play();
		_instance._swirl3.Play();
		_audioTweener?.Kill();
		_audioTweener = _audioSource.DOFade(0.35f, 0.5f);
	}

	public static void Stop()
	{
		_instance._swirl1.Stop();
		_instance._swirl2.Stop();
		_instance._swirl3.Stop();
		_audioTweener?.Kill();
		_audioTweener = _audioSource.DOFade(0f, 1f);
	}
}