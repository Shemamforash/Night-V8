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
            gameObject.AddComponent<Orbit>().Initialise(targetTransform, v => MovementController.AddForce(v), 10, 2f, 4f);
            gameObject.AddComponent<Beam>().Initialise(5f, 3f);
        }
    }
}