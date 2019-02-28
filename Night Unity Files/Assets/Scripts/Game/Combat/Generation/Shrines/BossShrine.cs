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
            List<EnemyType> allowedEnemies = WorldState.GetAllowedNightmareEnemyTypes();
            List<EnemyType> enemies = new List<EnemyType>();
            for (int i = 0; i < 500; ++i)
                enemies.Add(allowedEnemies.RandomElement());
            CombatManager.Instance().OverrideInactiveEnemies(enemies);
            CombatManager.Instance().OverrideMaxSize(currentSize);

            float roundTime = 60;
            float currentTime = roundTime;
            float sizeIncreaseTimer = 5f;

            while (currentTime > 0)
            {
                if (!CombatManager.Instance().IsCombatActive()) yield return null;
                UpdateCountdown(currentTime, roundTime);
                sizeIncreaseTimer -= Time.deltaTime;
                if (sizeIncreaseTimer < 0)
                {
                    sizeIncreaseTimer = 5f;
                    if (currentSize < maxSize) ++currentSize;
                    CombatManager.Instance().OverrideInactiveEnemies(enemies);
                    CombatManager.Instance().OverrideMaxSize(currentSize);
                }

                currentTime -= Time.deltaTime;
                yield return null;
            }

            Succeed();
            base.EndChallenge();
            CombatManager.Instance().ClearInactiveEnemies();
        }

        protected override void EndChallenge()
        {
            if (CombatManager.Instance().ClearOfEnemies())
                Succeed();
            else
                Fail();
            base.EndChallenge();
        }
    }
}