using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Crackling : MonoBehaviour
{
	public float VolumeMin = 0.2f, VolumeMax = 0.4f;
	public float CrackleRate = 1f;
	private AudioSource _audioSource;
	private float _volumeOverride = 1f;

	public void Awake ()
	{
		_audioSource = GetComponent<AudioSource>();
	}
	
	public void Update ()
	{
		float val = Mathf.PerlinNoise(Time.timeSinceLevelLoad * CrackleRate, 0);
		_audioSource.volume = Mathf.Lerp(VolumeMin, VolumeMax, val) * _volumeOverride;
	}

	public void FadeIn(float duration = 0.5f)
	{
		DOTween.To(() => _volumeOverride, f => _volumeOverride = f, 1, duration);
	}

	public void FadeOut(float duration = 0.5f)
	{
		DOTween.To(() => _volumeOverride, f => _volumeOverride = f, 0, duration);
	}

	public void Silence()
	{
		_volumeOverride = 0f;
	}
}
