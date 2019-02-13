using UnityEngine;

namespace Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours
{
    public class Drone : EnemyBehaviour
    {
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            MovementController.SetSpeed(Random.Range(3f, 7f));
            float rotateSpeed = Random.Range(10f, 20f);
            if (Random.Range(0, 2) == 0) rotateSpeed = -rotateSpeed;
            gameObject.AddComponent<Rotate>().RotateSpeed = rotateSpeed;
        }

        public void SetTarget(Transform targetTransform)
        {
            float speed = Random.Range(7f, 10f);
            float orbitMin = Random.Range(0.5f, 0.75f);
            float orbitMax = Random.Range(orbitMin + 0.1f, 1.5f);
            gameObject.AddComponent<Orbit>().Initialise(targetTransform, v => MovementController.AddForce(v), speed, orbitMin, orbitMax);
            Heavyshot shot = gameObject.AddComponent<Heavyshot>();
            float minTime = Random.Range(4f, 8f);
            float maxTime = Random.Range(minTime + 1f, 10f);
            shot.Initialise(maxTime, minTime, 3, 0.2f);
        }

        protected override void UpdateRotation()
        {
        }
    }
}