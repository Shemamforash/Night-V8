using UnityEngine;
using UnityEngine.Audio;

namespace Game.Gear.Weapons
{
    public class Spoolup : BaseWeaponBehaviour
    {
        private AudioSource _spinSource;
        private float _spoolUpLevel;
        private bool _spooling;
        private static AudioMixerGroup _modifiedMixerGroup;

        public void Awake()
        {
            _spinSource = gameObject.AddComponent<AudioSource>();
            if (_modifiedMixerGroup == null) _modifiedMixerGroup = Resources.Load<AudioMixer>("AudioMixer/Master").FindMatchingGroups("Modified")[0];
            _spinSource.outputAudioMixerGroup = _modifiedMixerGroup;
            _spinSource.loop = true;
        }

        protected bool ReadyToFire()
        {
            return _spoolUpLevel == 1;
        }

        public override void StartFiring()
        {
            _spooling = true;
            if (_spinSource.clip == null) _spinSource.clip = Origin.WeaponAudio.SpoolClip;
            _spinSource.Play();
            if (!ReadyToFire()) return;
            base.StartFiring();
            Fire();
        }

        public void Update()
        {
            if (!_spooling && _spoolUpLevel > 0)
            {
                _spoolUpLevel -= Time.deltaTime;
                if (_spoolUpLevel < 0) _spoolUpLevel = 0;
            }

            else if (_spooling && _spoolUpLevel < 1)
            {
                _spoolUpLevel += Time.deltaTime;
                if (_spoolUpLevel > 1) _spoolUpLevel = 1;
            }

            _spinSource.volume = _spoolUpLevel;
            _spinSource.pitch = _spoolUpLevel / 2f + 0.5f;
        }

        public override void StopFiring()
        {
            base.StopFiring();
            _spooling = false;
        }
    }
}