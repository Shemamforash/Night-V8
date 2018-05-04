using System.Collections;
using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Maelstrom : EnemyBehaviour
    {
        private const float DamageToSplit = 100;
        private float _damageTaken;

        private const int MinImagesReleased = 10;
        private const int MaxImagesReleased = 20;

        private const float ShotTimeMax = 5f;
        private float _currentTime;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            HealthController.AddOnTakeDamage(a =>
            {
                _damageTaken += a;
                if (_damageTaken < DamageToSplit) return;
                Split();
                _damageTaken = 0;
            });
        }

        private void Split()
        {
            int ghoulsToRelease = Random.Range(MinImagesReleased + 1, MaxImagesReleased + 1);
            for (int i = MinImagesReleased; i < ghoulsToRelease; ++i)
            {
                Cell c = PathingGrid.Instance().GetCellNearMe(CurrentCell(), 1f);
                EnemyBehaviour image = CombatManager.QueueEnemyToAdd(EnemyType.MaelstromImage);
                image.gameObject.transform.position = c.Position;
            }
        }

        public override void Update()
        {
            base.Update();
            _currentTime += Time.deltaTime;
            if (_currentTime < ShotTimeMax) return;
            _currentTime = 0;
            StartCoroutine(FireMaelstromShot());
        }

        private IEnumerator FireMaelstromShot()
        {
            GetComponent<ParticleSystem>().Play();
            float shotTime = GetComponent<ParticleSystem>().main.duration + 1;
            while (shotTime > 0)
            {
                shotTime -= Time.deltaTime;
                yield return null;
            }
            MaelstromShotBehaviour.Create(GetTarget().transform.position - transform.position, transform.position);
        }
    }
}