using System.Collections;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Ghast : EnemyBehaviour
    {
        private float _teleportCooldown;
        private const float TeleportCooldownMax = 5f;
        private ParticleSystem _teleportInParticles, _teleportOutParticles;
        private const float TeleportTimerMax = 0.5f;
        private float _teleportTimer;
        private bool _teleporting;

        public void Awake()
        {
            _teleportInParticles = Helper.FindChildWithName<ParticleSystem>(gameObject, "In");
            _teleportOutParticles = Helper.FindChildWithName<ParticleSystem>(gameObject, "Out");
        }
        
        public override void Update()
        {
            if (_teleporting) return;
            base.Update();
            UpdateTeleport();
        }

        private void UpdateTeleport()
        {
            _teleportCooldown -= Time.deltaTime;
            if (_teleportCooldown > 0f) return;
            _teleportCooldown = TeleportCooldownMax + Random.Range(0f, TeleportCooldownMax);
            StartCoroutine(Teleport());
        }

        private IEnumerator Teleport()
        {
            _teleporting = true;
            _teleportOutParticles.Play();
            _teleportTimer = 0f;
            transform.position = new Vector2(400, 400);
            
            while (_teleportTimer < TeleportTimerMax)
            {
                _teleportTimer += Time.deltaTime;
                yield return null;
            }

            Cell c = PathingGrid.Instance().GetCellNearMe(CombatManager.Player().CurrentCell(), 4);
            _teleportInParticles.Play();
            transform.position = c.transform.position;
            _teleporting = false;
        }
    }
}