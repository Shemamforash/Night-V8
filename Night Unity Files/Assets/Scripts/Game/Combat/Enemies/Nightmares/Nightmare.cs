using System.Collections.Generic;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Nightmare : NightmareEnemyBehaviour
    {
        private Beam _beam;
        private readonly List<EnemyBehaviour> _drones = new List<EnemyBehaviour>();

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            _beam = gameObject.AddComponent<Beam>();
            _beam.Initialise(15f, 10f);
            CurrentAction = Move;
        }

        public void Start()
        {
            int NumberOfDrones = (int) (WorldState.Difficulty() / 10f) + 2;
            for (int i = 0; i < NumberOfDrones; ++i)
            {
                EnemyBehaviour drone = CombatManager.QueueEnemyToAdd(EnemyType.Drone);
                _drones.Add(drone);
                drone.transform.position = AdvancedMaths.CalculatePointOnCircle(Random.Range(0, 360), 2f, transform.position);
                ((Drone) drone).SetTarget(transform);
            }
        }

        public override void Kill()
        {
            _drones.ForEach(d => { d.Kill(); });
            base.Kill();
        }

        protected override void UpdateRotation()
        {
            if (_beam.Active()) return;
            base.UpdateRotation();
        }
    }
}