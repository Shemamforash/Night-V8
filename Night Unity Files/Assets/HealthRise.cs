using System.Collections;
using System.Collections.Generic;
using SamsHelper.BaseGameFunctionality.Basic;
using UnityEngine;

public class HealthRise : MonoBehaviour
{
	private const float Speed = 1f;
	private const float LifeTime = 1f;
	private static readonly ObjectPool<HealthRise> _healthRisePool = new ObjectPool<HealthRise>("Prefabs/Combat/Heal Indicator");

	public static void Create(Vector3 position)
	{
		HealthRise healthRise = _healthRisePool.Create();
		healthRise.transform.position = position;
		healthRise.StartCoroutine(healthRise.MoveUp());
	}

	private IEnumerator MoveUp()
	{
		float age = 0f;
		while (age < LifeTime)
		{
			transform.Translate(0f, Speed * Time.deltaTime, 0f);
			age += Time.deltaTime;
			yield return null;
		}
	}
}
