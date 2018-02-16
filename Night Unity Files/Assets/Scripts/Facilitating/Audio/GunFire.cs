
using System.Collections;
using System.Collections.Generic;
using Game.Gear.Weapons;
using UnityEngine;

namespace Facilitating.Audio
{
    public class GunFire : MonoBehaviour
    {
        public AudioClip[] RifleFire;
        public AudioClip[] SMGFire;
        public AudioClip BoltPull;
        private static GunFire _instance;
        public GameObject AudioSourcePrefab;
        public AudioClip[] Steps;

        private static readonly List<PoolingAudioSource> _inactiveAudioSources = new List<PoolingAudioSource>();
        private static List<PoolingAudioSource> _activeSources = new List<PoolingAudioSource>();

        public void Awake()
        {
            _instance = this;
        }

        private class PoolingAudioSource
        {
            private readonly AudioSource _audioSource;
            private readonly GameObject _gameObject;

            public PoolingAudioSource()
            {
                _gameObject = Instantiate(_instance.AudioSourcePrefab, Vector3.zero, Quaternion.identity);
                _audioSource = _gameObject.GetComponent<AudioSource>();
                _inactiveAudioSources.Add(this);
            }

            public float PlayClip(float position, AudioClip sound)
            {
                _gameObject.transform.position = new Vector3(0, 0, position);
                _audioSource.pitch = Random.Range(0.9f, 1.1f);
                _audioSource.volume = Random.Range(0.9f, 1.0f);
                _audioSource.PlayOneShot(sound);
                _inactiveAudioSources.Remove(this);
                _activeSources.Add(this);
                return sound.length;
            }
        }

        private static PoolingAudioSource GetAvailableAudioSource()
        {
            return _inactiveAudioSources.Count == 0 ? new PoolingAudioSource() : _inactiveAudioSources[Random.Range(0, _inactiveAudioSources.Count)];
        }

        private IEnumerator ReuseAudioSource(float time, PoolingAudioSource audioSource)
        {
            yield return new WaitForSeconds(time);
            _activeSources.Remove(audioSource);
            _inactiveAudioSources.Add(audioSource);
        }

        private void PlaySound(float position, AudioClip sound)
        {
            PoolingAudioSource audioSource = GetAvailableAudioSource();
            float duration = audioSource.PlayClip(position, sound);
            StartCoroutine(ReuseAudioSource(duration, audioSource));
        }

        public static void Fire(WeaponType type, float distance)
        {
            _instance.PlaySound(distance, _instance.RifleFire[Random.Range(0, _instance.RifleFire.Length)]);
//
//            switch (type)
//            {
//                case WeaponType.Pistol:
//                    break;
//                case WeaponType.Rifle:
//                    CreateAudioSource(distance, _instance.RifleFire[Random.Range(0, _instance.RifleFire.Length)]);
//                    break;
//                case WeaponType.Shotgun:
//                    break;
//                case WeaponType.SMG:
//                    CreateAudioSource(distance, _instance.SMGFire[Random.Range(0, _instance.SMGFire.Length)]);
//                    break;
//                case WeaponType.LMG:
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
//            }
        }

        public static void Cock(float duration)
        {
//            _instance.GunSource.volume = 1;
//            _instance.GunSource.PlayOneShot(_instance.BoltPull, duration);
        }

        public static void Step(float distance)
        {
            _instance.PlaySound(distance, _instance.Steps[Random.Range(0, _instance.Steps.Length)]);
        }
    }
}