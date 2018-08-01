﻿using Game.Combat.Misc;
using UnityEngine;

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
            CurrentAction = () =>
            {
                Shot powerShot = Shot.Create(this);
                powerShot.LeaveFireTrail();
                powerShot.SetBurnChance(1);
                powerShot.Fire();
                ResetCooldown();
                TryFire();
                _firing = false;
            };
        }

        protected override void OnAlert()
        {
            base.OnAlert();
            ResetCooldown();
        }

        public override void Update()
        {
            base.Update();
            if (_firing || !Alerted) return;
            _powerShotCooldown -= Time.deltaTime;
            if (_powerShotCooldown < 0) FirePowerShot();
        }
    }
}