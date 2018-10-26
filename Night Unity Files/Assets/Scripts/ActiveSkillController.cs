using SamsHelper.Libraries;
using UnityEngine;

public class ActiveSkillController : MonoBehaviour
{

	private ParticleSystem _swirl1, _swirl2, _swirl3;
	private static ActiveSkillController _instance;

	private void Awake()
	{
		_swirl1 = gameObject.FindChildWithName<ParticleSystem>("Swirl 1");
		_swirl2 = gameObject.FindChildWithName<ParticleSystem>("Swirl 2");
		_swirl3 = gameObject.FindChildWithName<ParticleSystem>("Swirl 3");
		_instance = this;
	}

	public static void Play()
	{
		_instance._swirl1.Play();
		_instance._swirl2.Play();
		_instance._swirl3.Play();
	}

	public static void Stop()
	{
		_instance._swirl1.Stop();
		_instance._swirl2.Stop();
		_instance._swirl3.Stop();
	}
}
