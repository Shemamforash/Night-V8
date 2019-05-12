using System.Collections;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
	public class TeleportInOnly : MonoBehaviour
	{
		private static GameObject _teleportInPrefab;

		private void Awake()
		{
			StartCoroutine(PlayTeleportInEffect());
		}

		private IEnumerator PlayTeleportInEffect()
		{
			ParticleSystem teleportInParticles = GetComponent<ParticleSystem>();
			teleportInParticles.Emit(50);
			yield return new WaitForSeconds(1f);
			Destroy(gameObject);
		}

		public static void TeleportIn(Vector2 position)
		{
			if (_teleportInPrefab == null) _teleportInPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Teleport In");
			GameObject teleportIn                            = Instantiate(_teleportInPrefab);
			teleportIn.transform.position = position;
			teleportIn.AddComponent<TeleportInOnly>();
		}
	}
}