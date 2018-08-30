using System;
using Game.Combat.Enemies.Misc;
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
            SetActionText("Throwing Grenade");
            CurrentAction = null;
            SkillAnimationController.Create("Witch", 1f, () =>
            {
                Vector2 currentPosition = transform.position;
                Vector2 targetPosition = GetTarget().transform.position;
                switch (Random.Range(0, 3))
                {
                    case 0:
                        Grenade.CreateBasic(currentPosition, targetPosition);
                        break;
                    case 1:
                        Grenade.CreateBasic(currentPosition, targetPosition);
                        break;
                    case 2:
                        Grenade.CreateBasic(currentPosition, targetPosition);
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

        protected override void OnAlert()
        {
            base.OnAlert();
            ResetCooldown();
        }

        public override void Update()
        {
            base.Update();
            if (_throwing || !Alerted) return;
            _cooldownTime -= Time.deltaTime;
            if (_cooldownTime > 0) return;
            ThrowGrenade();
        }
    }
}