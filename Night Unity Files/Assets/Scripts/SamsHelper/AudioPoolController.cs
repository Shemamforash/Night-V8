using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPoolController : MonoBehaviour
{
	private readonly ObjectPool<InstancedAudio> _audioPool = new ObjectPool<InstancedAudio>("Audio", "Prefabs/AudioSource");
	private AudioMixerGroup _mixerGroup;
	private float _spatialBlend;

	public void SetMixerGroup(string groupName, float spatialBlend)
	{
		_mixerGroup = Resources.Load<AudioMixer>("AudioMixer/Master").FindMatchingGroups(groupName)[0];
		Assert.IsNotNull(_mixerGroup);
		_spatialBlend = spatialBlend;
	}

	public InstancedAudio Create()
	{
		Assert.IsNotNull(_mixerGroup);
		InstancedAudio instance = _audioPool.Create();
		instance.transform.SetParent(transform);
		instance.transform.localPosition = Vector2.zero;
		instance.SetMixerGroup(_audioPool, _mixerGroup, _spatialBlend);
		return instance;
	}
}
