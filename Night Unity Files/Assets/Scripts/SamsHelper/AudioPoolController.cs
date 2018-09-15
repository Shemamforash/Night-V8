using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

public class AudioPoolController : MonoBehaviour
{
	private ObjectPool<InstancedAudio> _audioPool = new ObjectPool<InstancedAudio>("Audio", "Prefabs/AudioSource");

	public void PlayClip(AudioClip clip, float volumeOffset = 0f, float hpfValue = 5000)
	{
		InstancedAudio instance = _audioPool.Create();
		instance.transform.SetParent(transform);
		instance.transform.localPosition = Vector2.zero;
		instance.Play(_audioPool, clip, volumeOffset, hpfValue);
	}
}
