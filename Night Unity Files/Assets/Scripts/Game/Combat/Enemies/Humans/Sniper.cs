using Game.Combat.Misc;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.Humans
{
    public class Sniper : ArmedBehaviour
    {
        private float _powerShotCooldown;
        private bool _firing;

        private void ResetCooldown()
        {
            _powerShotCooldown = Random.Range(5, 10);
            _firing = false;
        }

        private void FirePowerShot()
        {
            Shot powerShot = Shot.Create(this);
            powerShot.LeaveFireTrail();
            powerShot.SetBurnChance(1);
            powerShot.Fire();
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
            if (!Alerted || _firing) return;
            _powerShotCooldown -= Time.deltaTime;
            if (_powerShotCooldown > 0) return;
            CurrentAction = null;
            _firing = true;
            SkillAnimationController.Create(transform, "Sniper", 1f, FirePowerShot);
        }
    }
}