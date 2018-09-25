using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Misc
{
    public class EnemyCampfire
    {
        private readonly Vector2 FirePosition;

        public EnemyCampfire(Vector2 position)
        {
            FirePosition = position;
        }

        public void CreateObject()
        {
            FireGenerator.Create(FirePosition);
            PathingGrid.AddBlockingArea(FirePosition, 0.5f);
        }
    }
}