using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Nightmare : NightmareEnemyBehaviour
    {
        private const int HealthLostTarget = 200;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            Destroy(gameObject.GetComponent<MoveBehaviour>());
            gameObject.AddComponent<Feed>().Initialise(HealthLostTarget);
            gameObject.AddComponent<Bombardment>().Initialise(1, 0.5f, 0.2f);
        }

        public void Start()
        {
//            int numberOfDrones = Random.Range(2, 5);
            int numberOfDrones = 1;
            for (int i = 0; i < numberOfDrones; ++i)
            {
                EnemyBehaviour drone = CombatManager.QueueEnemyToAdd(EnemyType.Drone);
                drone.transform.position = AdvancedMaths.CalculatePointOnCircle(Random.Range(0, 360), 5f, transform.position);
                ((Drone) drone).SetTarget(transform);
            }
        }
    }
}