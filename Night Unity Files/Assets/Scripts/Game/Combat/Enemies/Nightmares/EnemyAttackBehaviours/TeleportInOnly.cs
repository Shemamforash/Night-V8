using System.Collections;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class TeleportInOnly : MonoBehaviour
    {
        private ParticleSystem _teleportInParticles;
        private static GameObject _teleportInPrefab;

        public void Awake()
        {
            if (_teleportInPrefab == null) _teleportInPrefab = Resources.Load<GameObject>("Prefabs/Combat/Teleport In");

            GameObject teleportIn = Instantiate(_teleportInPrefab);
            teleportIn.transform.SetParent(transform, false);
            _teleportInParticles = teleportIn.GetComponent<ParticleSystem>();
        }

        public static void TeleportObjectIn(GameObject g)
        {
            TeleportInOnly t = g.AddComponent<TeleportInOnly>();
            t.StartCoroutine(t.TeleportInEffect());
        }

        private IEnumerator TeleportInEffect()
        {
            _teleportInParticles.Emit(50);
            while (_teleportInParticles.particleCount > 0) yield return null;
            Destroy(_teleportInParticles.gameObject);
            Destroy(this);
        }
    }
}