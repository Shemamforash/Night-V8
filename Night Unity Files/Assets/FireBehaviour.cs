using System.Collections;
using System.Collections.Generic;
using LOS;
using SamsHelper;
using UnityEngine;

public class FireBehaviour : MonoBehaviour
{
	private const int MaxEmissionRate = 500;
	private const float LifeTime = 4f;
	private float _age;
	private ParticleSystem _particles;
	private LOSRadialLight _light;
	private static readonly List<FireBehaviour> _firePool = new List<FireBehaviour>();
	private static GameObject _firePrefab;

	public void Awake()
	{
		_particles = GetComponent<ParticleSystem>();
		_light = Helper.FindChildWithName<LOSRadialLight>(gameObject, "Light");
	}

	public static void StartBurning(Vector3 position)
	{
		FireBehaviour fire = GetNewFire();
		fire.StartCoroutine(fire.Burn(position));
	}

	private void OnDestroy()
	{
		_firePool.Remove(this);
	}

	private static FireBehaviour GetNewFire()
	{
		if (_firePool.Count == 0)
		{
			if (_firePrefab == null) _firePrefab = Resources.Load<GameObject>("Prefabs/Combat/Fire Area");
			GameObject fireObject = Instantiate(_firePrefab);
			fireObject.transform.localScale = Vector3.one;
			return fireObject.GetComponent<FireBehaviour>();
		}

		FireBehaviour fire = _firePool[0];
		_firePool.RemoveAt(0);
		fire.gameObject.SetActive(true);
		return fire;
	}

	private IEnumerator Burn(Vector3 position)
	{
		gameObject.SetActive(true);
		transform.position = position;
		_age = 0f;
		while (_age < LifeTime)
		{
			float normalisedTime = 1 - _age / LifeTime;
			ParticleSystem.EmissionModule emission = _particles.emission;
			emission.rateOverTime = (int)(normalisedTime * MaxEmissionRate);
			_light.color = new Color(0.6f, 0.1f, 0.1f, 0.6f * normalisedTime);
			_age += Time.deltaTime;
			yield return null;
		}
		gameObject.SetActive(false);
		_firePool.Add(this);
	}
}
