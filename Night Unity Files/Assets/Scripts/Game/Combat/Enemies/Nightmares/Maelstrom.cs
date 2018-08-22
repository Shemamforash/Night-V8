using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Player;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Maelstrom : EnemyBehaviour
    {
        private const int DamageToSplit = 100;

        private const int MinImagesReleased = 5;
        private const int MaxImagesReleased = 10;

        private const float ShotTimeMax = 5f;
        private const float ShotTimeMin = 3f;

//        private Split _split;
        private Heavyshot _shot;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
//            _split = gameObject.AddComponent<Split>();
//            _split.Initialise(MinImagesReleased, Random.Range(100, 300), EnemyType.Decoy, DamageToSplit, MaxImagesReleased);
            _shot = gameObject.AddComponent<Heavyshot>();
            _shot.Initialise(ShotTimeMax, ShotTimeMin, 10, 0.2f);
            gameObject.AddComponent<Orbit>().Initialise(PlayerCombat.Instance.transform, 2f, Random.Range(2.5f, 4f));
        }
    }
}