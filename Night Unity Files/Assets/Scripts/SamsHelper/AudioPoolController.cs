using NUnit.Framework;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioPoolController : MonoBehaviour
{
	private static   GameObject                 _audioPoolParent;
	private readonly ObjectPool<InstancedAudio> _audioPool = new ObjectPool<InstancedAudio>("Audio", "Prefabs/AudioSource");
	private          AudioMixerGroup            _mixerGroup;
	private          float                      _spatialBlend;

	public void SetMixerGroup(string groupName, float spatialBlend)
	{
		_mixerGroup = Resources.Load<AudioMixer>("AudioMixer/Master").FindMatchingGroups(groupName)[0];
		Assert.IsNotNull(_mixerGroup);
		_spatialBlend = spatialBlend;
	}

	public InstancedAudio Create(bool spawnAsChild = true)
	{
		Assert.IsNotNull(_mixerGroup);
		InstancedAudio instance          = _audioPool.Create();
		Transform      instanceTransform = instance.transform;
		Transform      parent;
		if (spawnAsChild)
		{
			parent = transform;
		}
		else
		{
			if (_audioPoolParent == null)
			{
				_audioPoolParent = new GameObject();
				_audioPoolParent.transform.SetParent(GameObject.Find("Dynamic").transform);
			}

			parent = _audioPoolParent.transform;
		}

		instanceTransform.SetParent(parent);
		instanceTransform.localPosition = Vector2.zero;
		instance.SetMixerGroup(_audioPool, _mixerGroup, _spatialBlend);
		return instance;
	}
}