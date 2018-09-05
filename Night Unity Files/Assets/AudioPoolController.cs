using System.Collections;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

public class AudioPoolController : MonoBehaviour
{
	private static ObjectPool<AudioPoolController> _audioPool = new ObjectPool<AudioPoolController>("Audio", "Prefabs/AudioSource");
	private AudioSource _audioSource;
	private AudioHighPassFilter _hpf;

	public void Awake()
	{
		_audioSource = GetComponent<AudioSource>();
		_hpf = GetComponent<AudioHighPassFilter>();
	}

	public static void PlayClip(AudioClip clip, Vector2 position, float volumeOffset = 0f, float hpfValue = 5000)
	{
		AudioPoolController instance = _audioPool.Create();
		instance.transform.position = position;
		instance._hpf.cutoffFrequency = hpfValue;
		instance.SetClip(clip, volumeOffset);
	}

	private void SetClip(AudioClip clip, float volumeOffset)
	{
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
