using System.Collections;
using Game.Combat.Misc;
using SamsHelper.BaseGameFunctionality.Basic;

namespace Game.Gear.Weapons
{
    public class DoubleFireDelay : BaseWeaponBehaviour
    {
        private bool _stillFiring;
        
        public override void StartFiring()
        {
            base.StartFiring();
            if (_stillFiring) return;
            StartCoroutine(SecondaryFire());
        }

        private IEnumerator SecondaryFire()
        {
            Fire();
            _stillFiring = true;
            while (!FireRateTargetMet())
            {
                yield return null;
            }
            Fire();
            _stillFiring = false;
        }
    }
}