using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Generation.Shrines
{
    public class BossShrine : ShrineBehaviour
    {
        protected override void StartChallenge()
        {
            StartCoroutine(SpawnBoss());
        }

        protected override string GetInstructionText() => "Survive";

        private IEnumerator SpawnBoss()
        {
            int maxSize = (int) (WorldState.Difficulty() / 5f + 5);
            int currentSize = 1;
            List<EnemyTemplate> allowedEnemies = WorldState.GetAllowedNightmareEnemyTypes();
            List<Enemy> enemies = new List<Enemy>();
            for (int i = 0; i < 500; ++i)
                enemies.Add(new Enemy(allowedEnemies.RandomElement()));
            CombatManager.OverrideMaxSize(currentSize, enemies);

            float roundTime = 60;
            float currentTime = roundTime;
            float sizeIncreaseTimer = 5f;

            while (currentTime > 0)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
                UpdateCountdown(currentTime, roundTime);
                sizeIncreaseTimer -= Time.deltaTime;
                if (sizeIncreaseTimer < 0)
                {
                    sizeIncreaseTimer = 5f;
                    if (currentSize < maxSize) ++currentSize;
                    CombatManager.OverrideMaxSize(currentSize, enemies);
                }

                currentTime -= Time.deltaTime;
                yield return null;
            }

            Succeed();
            base.EndChallenge();
            CombatManager.OverrideMaxSize(0, new List<Enemy>());
        }

        protected override void EndChallenge()
        {
            if (CombatManager.ClearOfEnemies())
                Succeed();
            else
                Fail();
            base.EndChallenge();
        }
    }
}