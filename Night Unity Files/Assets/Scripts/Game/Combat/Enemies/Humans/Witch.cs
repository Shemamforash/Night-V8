using Game.Combat.Enemies.Misc;
using Game.Global;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.Humans
{
    public class Witch : ArmedBehaviour
    {
        private float _cooldownTime;
        private int _damageTaken;
        private bool _throwing;

        private void ThrowGrenade()
        {
            _throwing = true;
            CurrentAction = null;
            SkillAnimationController.Create(transform, "Witch", 1f, () =>
            {
                Vector2 currentPosition = transform.position;
                Vector2 targetPosition = TargetTransform().position;
                int max = 1;
                if (WorldState.Difficulty() > 10) max = 2;
                if (WorldState.Difficulty() > 20) max = 4;
                switch (Random.Range(0, max))
                {
                    case 0:
                        Grenade.CreateBasic(currentPosition, targetPosition, false);
                        break;
                    case 1:
                        Grenade.CreateIncendiary(currentPosition, targetPosition, false);
                        break;
                    case 2:
                        Grenade.CreateDecay(currentPosition, targetPosition, false);
                        break;
                    case 3:
                        Grenade.CreateSickness(currentPosition, targetPosition, false);
                        break;
                }

                ResetCooldown();
                _throwing = false;
                TryFire();
            });
        }

        private void ResetCooldown()
        {
            _cooldownTime = Random.Range(5, 10);
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            if (_throwing) return;
            _cooldownTime -= Time.deltaTime;
            if (_cooldownTime > 0) return;
            ThrowGrenade();
        }
    }
}