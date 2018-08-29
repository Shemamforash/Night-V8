using Game.Combat.Misc;
using Game.Combat.Player;
using UnityEngine;

namespace Game.Gear.Weapons
{
    public class Spoolup : BaseWeaponBehaviour
    {
        private const float SpoolUpTime = 1f;
        private float _currentTime;
        private bool SpinningUp, Spinning, SpinningDown;
        private AudioSource _spinSource;
        private AudioSource _weaponAudioSource;

        public void Awake()
        {
            _spinSource = gameObject.AddComponent<AudioSource>();
        }

        protected bool SpooledUp()
        {
            return _currentTime > SpoolUpTime;
        }

        public override void StartFiring(CharacterCombat origin)
        {
            if (!SpinningUp)
            {
                _spinSource.clip = origin.WeaponAudio.SpoolUpClip;
                _spinSource.loop = false;
                _spinSource.Play();
                SpinningUp = true;
            }
            
            if (_currentTime < SpoolUpTime)
            {
                _currentTime += Time.deltaTime;
                if (_currentTime <= SpoolUpTime) return;
                _spinSource.clip = origin.WeaponAudio.SpoolClip;
                _spinSource.loop = true;
                _spinSource.Play();
                return;
            }

            base.StartFiring(origin);
        }

        public override void StopFiring()
        {
            base.StopFiring();
            _currentTime = 0f;
            _spinSource.clip = Origin.WeaponAudio.SpoolDownClip;
            _spinSource.loop = false;
            _spinSource.time = Mathf.Min(1f - _currentTime, _spinSource.clip.length - 0.01f);
            _spinSource.Play();
            SpinningUp = false;
        }
    }
}