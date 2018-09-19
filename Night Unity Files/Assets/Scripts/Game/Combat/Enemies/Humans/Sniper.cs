using Game.Combat.Misc;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Combat.Enemies.Humans
{
    public class Sniper : ArmedBehaviour
    {
        private bool _firing;
        private float _powerShotCooldown;

        private void ResetCooldown()
        {
            _powerShotCooldown = Random.Range(5, 10);
        }

        private void FirePowerShot()
        {
            _firing = true;
            FacePlayer = true;
            Shot powerShot = Shot.Create(this);
            powerShot.LeaveFireTrail();
            powerShot.SetBurnChance(1);
            powerShot.Fire();
            ResetCooldown();
            TryFire();
            _firing = false;
        }

        protected override void OnAlert()
        {
            base.OnAlert();
            ResetCooldown();
        }

        public override void MyUpdate()
        {
            base.MyUpdate();
            if (_firing || !Alerted) return;
            _powerShotCooldown -= Time.deltaTime;
            if (_powerShotCooldown > 0) return;
            CurrentAction = null;
//            SkillAnimationController.Create("Sniper", 1f, FirePowerShot);
        }
    }
}