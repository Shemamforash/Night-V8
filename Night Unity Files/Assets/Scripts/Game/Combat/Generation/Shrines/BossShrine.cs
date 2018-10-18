using System.Collections;
using Game.Combat.Enemies;
using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Generation.Shrines
{
    public class BossShrine : ShrineBehaviour
    {
        protected override void StartShrine()
        {
            base.StartShrine();
            StartCoroutine(SpawnBoss());
        }

        private IEnumerator SpawnBoss()
        {
            float roundTime = 60;
            float currentTime = roundTime;

            EnemyBehaviour b = GenerateBoss(transform.position);
            AddEnemy(b);


            while (currentTime > 0)
            {
                if (!CombatManager.IsCombatActive()) yield return null;
                currentTime -= Time.deltaTime;

                UpdateCountdown(currentTime, roundTime);
                if (EnemiesDead())
                {
                    Succeed();
                    break;
                }

                yield return null;
            }

            EndChallenge();
        }

        private static T AddComponentOnce<T>(EnemyBehaviour b) where T : MonoBehaviour
        {
            T existing = b.gameObject.GetComponent<T>();
            return existing != null ? existing : b.gameObject.AddComponent<T>();
        }

        public static EnemyBehaviour GenerateBoss(Vector2 position)
        {
            EnemyType type;
            switch (Random.Range(0, 5))
            {
                case 0:
                    type = EnemyType.GhoulMother;
                    break;
                case 1:
                    type = EnemyType.Nightmare;
                    break;
                case 2:
                    type = EnemyType.Ghast;
                    break;
                case 3:
                    type = EnemyType.Maelstrom;
                    break;
                default:
                    type = EnemyType.Revenant;
                    break;
            }

            EnemyBehaviour boss = CombatManager.SpawnEnemy(type, position);
            switch (Random.Range(0, 3))
            {
                case 0:
                    AddComponentOnce<ErraticDash>(boss);
                    break;
                case 1:
                    AddComponentOnce<Orbit>(boss).Initialise(PlayerCombat.Instance.transform, v => boss.MovementController.AddForce(v), 10, 2, 5);
                    break;
                case 2:
                    AddComponentOnce<Teleport>(boss).Initialise(5, 2);
                    break;
            }

            AddWeapon(boss);
            AddWeapon(boss);
            return boss;
        }

        private static void AddWeapon(EnemyBehaviour boss)
        {
            switch (Random.Range(0, 7))
            {
                case 0:
                    int count = Random.Range(1, 5);
                    AddComponentOnce<Bombardment>(boss).Initialise(count, count, count / 2f);
                    break;
                case 1:
                    AddComponentOnce<Heavyshot>(boss).Initialise(6, 3);
                    break;
                case 2:
                    AddComponentOnce<LeaveFireTrail>(boss).Initialise();
                    break;
                case 3:
                    AddComponentOnce<Push>(boss).Initialise(10, 3);
                    break;
                case 4:
                    AddComponentOnce<Split>(boss).Initialise(1, 6, EnemyType.Ghoul, 50, 2);
                    break;
                case 5:
                    AddComponentOnce<Spawn>(boss).Initialise(EnemyType.Ghoul, 10, 2, 5);
                    break;
            }
        }

        protected override void EndChallenge()
        {
            if (EnemiesDead())
            {
                Succeed();
            }
            else
            {
                Fail();
            }

            base.EndChallenge();
        }
    }
}