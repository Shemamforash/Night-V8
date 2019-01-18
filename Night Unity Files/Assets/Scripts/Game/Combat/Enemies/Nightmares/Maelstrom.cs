using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Maelstrom : NightmareEnemyBehaviour
    {
        private const int DamageToSplit = 100;

        private const int MinImagesReleased = 5;
        private const int MaxImagesReleased = 10;

        private const float ShotTimeMax = 5f;
        private const float ShotTimeMin = 3f;

        private Heavyshot _shot;
        private Split _split;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            if (WorldState.Difficulty() >= 25)
            {
                _split = gameObject.AddComponent<Split>();
                _split.Initialise(MinImagesReleased, Random.Range(100, 300), EnemyType.Decoy, DamageToSplit, MaxImagesReleased);
            }

            _shot = gameObject.AddComponent<Heavyshot>();
            _shot.Initialise(ShotTimeMax, ShotTimeMin, 10, 0.2f);
            gameObject.AddComponent<Orbit>().Initialise(PlayerCombat.Instance.transform, v => MovementController.AddForce(v), 4, 2f, Random.Range(2.5f, 4f));
        }

        public override void Kill()
        {
            MaelstromShotBehaviour.CreateBurst(60, (Vector2) transform.position, 1f, Random.Range(0, 360));
            base.Kill();
        }
    }
}