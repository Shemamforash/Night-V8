using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
    public class Mountain : ArmedBehaviour
    {
        private bool _pushing;
        private float _forceCooldown;
        private const float MinDistanceToTarget = 1.5f;

        private void ResetCooldown()
        {
            _forceCooldown = Random.Range(5, 10);
            _pushing = false;
        }

        private void Push()
        {
            PushController.Create(transform.position, 0f);
            PushController.Create(transform.position, 90f);
            PushController.Create(transform.position, 180f);
            PushController.Create(transform.position, 270f);
            ResetCooldown();
            TryFire();
        }

        protected override void OnAlert()
        {
            base.OnAlert();
            ResetCooldown();
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            if (!Alerted || _pushing) return;
            _forceCooldown -= Time.deltaTime;
            if (_forceCooldown > 0) return;
            if (DistanceToTarget() > MinDistanceToTarget) return;
            CurrentAction = null;
            _pushing = true;
            //todo mountain skill image
            SkillAnimationController.Create(transform, "Sniper", 1f, Push);
        }
    }
}