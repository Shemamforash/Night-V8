using System.Collections;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class TeleportInOnly : MonoBehaviour
    {
        private ParticleSystem _teleportInParticles;
        private static GameObject _teleportInPrefab;

        private void Awake()
        {
            StartCoroutine(PlayTeleportInEffect());
            _teleportInParticles = GetComponent<ParticleSystem>();
        }

        private IEnumerator PlayTeleportInEffect()
        {
            _teleportInParticles.Emit(50);
            while (_teleportInParticles.particleCount > 0) yield return null;
            Destroy(gameObject);
        }

        public static void TeleportIn(Vector2 position)
        {
            if (_teleportInPrefab == null) _teleportInPrefab = Resources.Load<GameObject>("Prefabs/Combat/Visuals/Teleport In");
            GameObject teleportIn = Instantiate(_teleportInPrefab);
            teleportIn.transform.position = position;
        }
    }
}