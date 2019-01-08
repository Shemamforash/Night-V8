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
        private const float GhoulSpawnRate = 2f;
        private float _timeToNextGhoul;
        private float _timeAlive;

        public void UpdateGhoulSpawn(int armCount)
        {
            _timeAlive += Time.deltaTime;
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
            int MaxGhouls = (int) (_timeAlive / 6f);
            if (MaxGhouls > 10) MaxGhouls = 10;
            if (_ghouls.Count == MaxGhouls) return;
            _timeToNextGhoul -= Time.deltaTime;
            if (_timeToNextGhoul > 0f) return;
            EnemyBehaviour enemy = CombatManager.SpawnEnemy(EnemyType.Ghoul, AdvancedMaths.RandomDirection() * 9);
            _ghouls.Add(enemy);
            if (_timeAlive > 60 && Helper.RollDie(0, 3))
            {
                LeaveFireTrail fireTrail = enemy.gameObject.AddComponent<LeaveFireTrail>();
                fireTrail.Initialise();
                fireTrail.AddIgnoreTargets(StarfishBehaviour.Instance().Sections);
            }

            _timeToNextGhoul = Random.Range(GhoulSpawnRate, GhoulSpawnRate * 2f);
        }
    }
}