﻿using System.Collections;
using Game.Combat.Misc;

namespace Game.Gear.Weapons
{
    public class DoubleFireDelay : BaseWeaponBehaviour
    {
        private bool _stillFiring;
        
        public override void StartFiring(CharacterCombat origin)
        {
            base.StartFiring(origin);
            if (_stillFiring) return;
            StartCoroutine(SecondaryFire());
        }

        private IEnumerator SecondaryFire()
        {
            Fire(Origin);
            _stillFiring = true;
            while (!FireRateTargetMet())
            {
                yield return null;
            }

            Fire(Origin);
            _stillFiring = false;
        }
    }
}