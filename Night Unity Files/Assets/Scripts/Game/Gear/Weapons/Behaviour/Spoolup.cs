using Game.Combat.Misc;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class Spoolup : BaseWeaponBehaviour
    {
        private const float SpoolUpTime = 1f;
        private float _currentTime;

        protected bool SpooledUp()
        {
            return _currentTime > SpoolUpTime;
        }
        
        public override void StartFiring(CharacterCombat origin)
        {
            if (_currentTime < SpoolUpTime)
            {
                _currentTime += Time.deltaTime;
                return;
            }
            base.StartFiring(origin);
        }

        public override void StopFiring()
        {
            base.StopFiring();
            _currentTime = 0f;
        }
    }
}