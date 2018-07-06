using Game.Combat.Generation;
using Game.Combat.Player;
using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
    public class Brawler : UnarmedBehaviour
    {
        private const float MinMeleeDistance = 1;
        private const float MeleeWarmupDuration = 1f;
        private const float MeleeDamage = 20;
        private const float MeleeCooldownDuration = 3f;
        private bool _meleeing;
        private const float MeleeForce = 200;

        public override void ChooseNextAction()
        {
            CurrentAction = MoveToPlayer;
        }

        public override void Update()
        {
            base.Update();
            if (!Alerted) return;
            if (_meleeing) return;
            if (DistanceToTarget() > MinMeleeDistance) return;
            StrikePlayer();
        }

        private void StrikePlayer()
        {
            float warmupDuration = MeleeWarmupDuration;
            float cooldownDuration = MeleeCooldownDuration;
            bool hasMeleed = false;
            _meleeing = true;
            CurrentAction = () =>
            {
                if (!_meleeing) return;
                if (warmupDuration > 0)
                {
                    warmupDuration -= Time.deltaTime;
                    return;
                }
                if (!hasMeleed)
                {
                    Ram(GetTarget(), MeleeForce);
                    hasMeleed = true;
                }
                else
                {
                    if (cooldownDuration > 0)
                    {
                        cooldownDuration -= Time.deltaTime;
                        return;
                    }

                    Cell target =  PathingGrid.GetCellNearMe(CurrentCell(), 2f);
                    GetRouteToCell(target);
                    _meleeing = false;
                }
            };
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("Player")) return;
            if (GetComponent<Rigidbody2D>().velocity.magnitude < 1f) return;
            other.gameObject.GetComponent<PlayerCombat>().HealthController.TakeDamage(MeleeDamage);
        }
    }
}