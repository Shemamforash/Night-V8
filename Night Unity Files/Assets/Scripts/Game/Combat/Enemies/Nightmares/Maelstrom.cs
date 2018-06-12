using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class Maelstrom : EnemyBehaviour
    {
        private const int DamageToSplit = 100;

        private const int MinImagesReleased = 10;
        private const int MaxImagesReleased = 20;

        private const float ShotTimeMax = 5f;
        private Split _split;
        private Heavyshot _shot;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            _split = gameObject.AddComponent<Split>();
            _split.Initialise(MinImagesReleased, Random.Range(100, 300), EnemyType.Decoy, DamageToSplit, MaxImagesReleased);
            _shot = gameObject.AddComponent<Heavyshot>();
            _shot.Initialise(ShotTimeMax);
        }
    }
}