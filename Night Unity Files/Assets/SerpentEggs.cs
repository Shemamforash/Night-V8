using System.Collections;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using SamsHelper.Libraries;
using UnityEngine;

public class SerpentEggs : TimedAttackBehaviour {
	private Transform _spawnTransform;

	public void Initialise(float maxTime, float minTimer, Transform spawnTransform)
	{
		Initialise(maxTime, minTimer);
		_spawnTransform = spawnTransform;
	}
	
	protected override void Attack()
	{
		StartCoroutine(Spawn());
	}

	private IEnumerator Spawn()
	{
		for (int i = 0; i < Random.Range(3, 6); ++i) {
			float timer = Random.Range(0.25f, 0.5f);
			while (timer > 0f)
			{
				timer -= Time.deltaTime;
				yield return null;
			}
			SerpentEggBehaviour.Create(AdvancedMaths.RandomVectorWithinRange(_spawnTransform.position, 0.2f));
			yield return null;
		}
	}
}
