using SamsHelper.Libraries;
using UnityEngine;

public class UiCompassController : MonoBehaviour
{
	private static ParticleSystem _compassPulse;
	private GameObject _indicatorPrefab;

	public void Awake()
	{
		_indicatorPrefab = Resources.Load<GameObject>("Prefabs/Combat/Indicator");
		_compassPulse = Helper.FindChildWithName<ParticleSystem>(gameObject, "Compass Pulse");
	}

	public static void EmitPulse()
	{
		_compassPulse.Play();
	}

}
