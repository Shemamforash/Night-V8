using Game.Combat.Misc;
using UnityEngine;

namespace Game.Combat.Enemies.Humans
{
    public class Sniper : EnemyBehaviour
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
            CurrentAction = () =>
            {
                Shot powerShot = Shot.Create(this);
                powerShot.ActivateFireTrail();
                powerShot.SetBurnChance(1);
                powerShot.Fire();
                ResetCooldown();
                ChooseNextAction();
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