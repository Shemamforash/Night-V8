using System;
using Game.Gear.Weapons;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Facilitating.Audio
{
    public class GunFire : MonoBehaviour
    {
        private AudioSource GunSource;
        public AudioClip[] RifleFire;
        public AudioClip[] SMGFire;
        public AudioClip BoltPull;
        private static GunFire _instance;
        public GameObject AudioSourcePrefab;
        public AudioClip[] Steps;

        public void Awake()
        {
            _instance = this;
            GunSource = GetComponent<AudioSource>();
        }

        private static void CreateAudioSource(float position, AudioClip sound)
        {
            //todo audiosource pool
            GameObject g = Instantiate(_instance.AudioSourcePrefab, new Vector3(0, 0, -position), Quaternion.identity);
            g.GetComponent<AudioSource>().PlayOneShot(sound);
            Destroy(g, sound.length);
        }
        
        public static void Fire(WeaponType type, float distance)
        {
            _instance.GunSource.pitch = Random.Range(0.9f, 1.1f);
            float volume = Random.Range(0.9f, 1f);
            _instance.GunSource.volume = volume;
            CreateAudioSource(distance, _instance.RifleFire[Random.Range(0, _instance.RifleFire.Length)]);
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
            _instance.GunSource.volume = 1;
            _instance.GunSource.PlayOneShot(_instance.BoltPull, duration);
        }

        public static void Step(float distance)
        {
            CreateAudioSource(distance, _instance.Steps[Random.Range(0, _instance.Steps.Length)]);
        }
    }
}