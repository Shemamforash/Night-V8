using Game.Combat.Enemies.Nightmares.EnemyAttackBehaviours;
using Game.Combat.Player;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;

namespace Game.Combat.Enemies.Nightmares
{
    public class GhoulMother : NightmareEnemyBehaviour
    {
        private const int MinGhoulsReleased = 2;
        private const int MaxGhoulsReleased = 4;
        private const float GhoulCooldownMax = 10f;
        private float _ghoulCooldown;
        private bool _pushAllowed;
        private float _pushCooldown;

        public override void Initialise(Enemy enemy)
        {
            base.Initialise(enemy);
            gameObject.AddComponent<Spawn>().Initialise(EnemyType.Ghoul, GhoulCooldownMax, MinGhoulsReleased, MaxGhoulsReleased);
            if (WorldState.Difficulty() < 25) return;
            _pushAllowed = true;
        }

        public void Update()
        {
            if (!_pushAllowed) return;
            if (_pushCooldown > 0f)
            {
                _pushCooldown -= Time.deltaTime;
                return;
            }

            if (transform.position.Distance(PlayerCombat.Position()) > 1.5f) return;
            _pushCooldown = 1f;
            PushController.Create(transform.position, 0f, false, 360f);
        }
    }
}