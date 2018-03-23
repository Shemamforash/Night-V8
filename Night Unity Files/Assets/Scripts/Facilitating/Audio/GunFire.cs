
using System.Collections;
using System.Collections.Generic;
using Game.Gear.Weapons;
using UnityEngine;

namespace Facilitating.Audio
{
    public class GunFire : MonoBehaviour
    {
        public AudioClip[] RifleFire;
        public AudioClip[] ShotgunFire;
        public AudioClip Explosion, Tinnitus;
        public AudioClip[] SMGFire;
        public AudioClip BoltPull;
        private static GunFire _instance;
        public GameObject AudioSourcePrefab;
        public AudioClip[] Steps;

        private static readonly List<PoolingAudioSource> _audioSourcePool = new List<PoolingAudioSource>();

        public void Awake()
        {
            _instance = this;
        }

        private class PoolingAudioSource : MonoBehaviour
        {
            private AudioSource _audioSource;

            public static PoolingAudioSource GetAudioSource()
            {
                PoolingAudioSource pooledAudio;
                if (_audioSourcePool.Count == 0)
                {
                    GameObject audioObject = Instantiate(_instance.AudioSourcePrefab, Vector3.zero, Quaternion.identity);
                    pooledAudio = audioObject.AddComponent<PoolingAudioSource>();
                    pooledAudio._audioSource = audioObject.GetComponent<AudioSource>();
                    _audioSourcePool.Add(pooledAudio);
                }
                else
                {
                    pooledAudio = _audioSourcePool[_audioSourcePool.Count - 1];
                    _audioSourcePool.RemoveAt(_audioSourcePool.Count - 1);
                }

                return pooledAudio;
            }

            public float PlayClip(float position, AudioClip sound, float maxDistance = 200)
            {
                transform.position = new Vector3(0, 0, position);
                _audioSource.pitch = Random.Range(0.9f, 1.1f);
                _audioSource.volume = Random.Range(0.9f, 1.0f);
                _audioSource.maxDistance = maxDistance;
                _audioSource.PlayOneShot(sound);
                _audioSourcePool.Remove(this);
                return sound.length;
            }

            public void OnDestroy()
            {
                _audioSourcePool.Remove(this);
            }
        }

        private IEnumerator ReuseAudioSource(float time, PoolingAudioSource audioSource)
        {
            yield return new WaitForSeconds(time);
            _audioSourcePool.Add(audioSource);
        }

        public static void EnterCover()
        {
//            _instance.gameObject.GetComponent<AudioLowPassFilter>().enabled = true;
        }
        
        public static void ExitCover()
        {
//            _instance.gameObject.GetComponent<AudioLowPassFilter>().enabled = false;
        }
        
        private void PlaySound(float position, AudioClip sound)
        {
            PoolingAudioSource audioSource = PoolingAudioSource.GetAudioSource();
            float duration = audioSource.PlayClip(position, sound);
            StartCoroutine(ReuseAudioSource(duration, audioSource));
        }

        public static void Explode(float distance)
        {
            PoolingAudioSource source = PoolingAudioSource.GetAudioSource();
            float duration =  source.PlayClip(distance, _instance.Explosion, 500);
            _instance.StartCoroutine(_instance.ReuseAudioSource(duration, source));

            source = PoolingAudioSource.GetAudioSource();
            duration =  source.PlayClip(distance, _instance.Tinnitus, 10);
            _instance.StartCoroutine(_instance.ReuseAudioSource(duration, source));
        }
        
        public static void Fire(WeaponType type, float distance)
        {

            switch (type)
            {
                case WeaponType.Pistol:
                    break;
                case WeaponType.Rifle:
                    _instance.PlaySound(distance, _instance.RifleFire[Random.Range(0, _instance.RifleFire.Length)]);
                    break;
                case WeaponType.Shotgun:
                    _instance.PlaySound(distance, _instance.ShotgunFire[Random.Range(0, _instance.ShotgunFire.Length)]);
                    break;
                case WeaponType.SMG:
                    _instance.PlaySound(distance, _instance.SMGFire[Random.Range(0, _instance.SMGFire.Length)]);
                    break;
                case WeaponType.LMG:
                    break;
            }
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