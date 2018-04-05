using System;
using Game.Combat.Enemies.EnemyTypes.Misc;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.EnemyTypes.Humans
{
    public class Witch : EnemyBehaviour
    {
        private int _damageTaken;
        private bool _throwing;
        private float _cooldownTime;

        private Action ThrowGrenade()
        {
            _throwing = true;
            float throwDuration = 1f;
            SetActionText("Throwing Grenade");
            return () =>
            {
                throwDuration -= Time.deltaTime;
                if (throwDuration > 0) return;
                Vector2 currentPosition = transform.position;
                //todo get player
                Vector2 targetPosition = GetTarget().transform.position;
                switch (Random.Range(0, 3))
                {
                    case 0:
                        Grenade.Create(currentPosition, targetPosition);
                        break;
                    case 1:
                        Grenade.Create(currentPosition, targetPosition);
                        break;
                    case 2:
                        Grenade.Create(currentPosition, targetPosition);
                        break;
                }

                ResetCooldown();
                _throwing = false;
                ChooseNextAction();
            };
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
            if (CouldHitTarget)
                CurrentAction = ThrowGrenade();
        }
    }
}