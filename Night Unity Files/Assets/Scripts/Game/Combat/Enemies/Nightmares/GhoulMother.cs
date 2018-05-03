using Game.Combat.Generation;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class GhoulMother : EnemyBehaviour
    {
        private const int MinGhoulsReleased = 3;
        private const int MaxGhoulsReleased = 6;
        private const float GhoulCooldownMax = 10f;
        private float _ghoulCooldown;

        public override void Update()
        {
            base.Update();
            _ghoulCooldown -= Time.deltaTime;
            if (_ghoulCooldown > 0) return;
            _ghoulCooldown = GhoulCooldownMax;
            ReleaseGhouls();
        }

        private void ReleaseGhouls()
        {
            int ghoulsToRelease = Random.Range(MinGhoulsReleased + 1, MaxGhoulsReleased + 1);
            Debug.Log("trying to release ghouls");
            for (int i = MinGhoulsReleased; i < ghoulsToRelease; ++i)
            {
                Cell c = PathingGrid.Instance().GetCellNearMe(CurrentCell(), 2f);
                EnemyBehaviour ghoul = CombatManager.QueueEnemyToAdd(EnemyType.Ghoul);
                ghoul.gameObject.transform.position = c.Position;
                Debug.Log("released ghoul");
            }
        }
    }
}