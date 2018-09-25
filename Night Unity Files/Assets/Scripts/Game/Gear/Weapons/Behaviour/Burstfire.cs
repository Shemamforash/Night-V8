using System.Collections;

namespace Game.Gear.Weapons
{
    public class Burstfire : BaseWeaponBehaviour
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
            for (int i = 0; i < 3; ++i)
            {
                while (!FireRateTargetMet())
                {
                    yield return null;
                }
                Fire();
            }
            _stillFiring = false;
        }
    }
}