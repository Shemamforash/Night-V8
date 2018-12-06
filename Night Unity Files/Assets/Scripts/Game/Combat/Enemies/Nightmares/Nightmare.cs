using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Nightmare : NightmareEnemyBehaviour
    {
        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            gameObject.AddComponent<Feed>().Initialise(15f, 10f);
            if(WorldState.Difficulty() > 25) gameObject.AddComponent<Beam>().Initialise(15f, 10f);
        }

        public void Start()
        {
            int NumberOfDrones = (int) (WorldState.Difficulty() / 10f);
            for (int i = 0; i < NumberOfDrones; ++i)
            {
                EnemyBehaviour drone = CombatManager.QueueEnemyToAdd(EnemyType.Drone);
                drone.transform.position = AdvancedMaths.CalculatePointOnCircle(Random.Range(0, 360), 5f, transform.position);
                ((Drone) drone).SetTarget(transform);
            }
        }
    }
}