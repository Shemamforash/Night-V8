using System.Collections.Generic;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Generation;
using Game.Combat.Misc;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Bosses.Starfish
{
    public class StarfishGhoulSpawn : MonoBehaviour
    {
        private readonly List<CanTakeDamage> _ghouls = new List<CanTakeDamage>();
        private const int MaxGhouls = 15;
        private float _ghoulSpawnRate;
        private float _timeToNextGhoul;

        public void UpdateGhoulSpawn(int armCount)
        {
            _ghoulSpawnRate = -2f * armCount / 55f + 2f;
            if (_ghoulSpawnRate < 0) _ghoulSpawnRate = 0;
            UpdateAliveGhouls();
            UpdateSpawn();
        }

        private void UpdateAliveGhouls()
        {
            for (int i = _ghouls.Count - 1; i >= 0; --i)
                if (_ghouls[i] == null)
                    _ghouls.RemoveAt(i);
        }

        private void UpdateSpawn()
        {
            if (_ghouls.Count == MaxGhouls) return;
            if (_ghoulSpawnRate == 0) return;
            _timeToNextGhoul -= Time.deltaTime;
            if (_timeToNextGhoul > 0f) return;
            EnemyBehaviour enemy = CombatManager.SpawnEnemy(EnemyType.Ghoul, AdvancedMaths.RandomDirection() * 9);
            _ghouls.Add(enemy);
            enemy.gameObject.AddComponent<LeaveFireTrail>().Initialise();
            _timeToNextGhoul = Random.Range(_ghoulSpawnRate, _ghoulSpawnRate * 2f);
        }
    }
}