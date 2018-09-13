using System.Collections;
using UnityEngine;

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
            float time = 0.25f;
            while (time > 0f)
            {
                time -= Time.deltaTime;
                yield return null;
            }

            Fire();
            _stillFiring = false;
        }
    }
}