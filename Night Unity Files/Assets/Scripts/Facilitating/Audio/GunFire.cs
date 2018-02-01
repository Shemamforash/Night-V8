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

        public void Awake()
        {
            _instance = this;
            GunSource = GetComponent<AudioSource>();
        }
        
        public static void Fire(WeaponType type, float normalisedDistance)
        {
            _instance.GunSource.pitch = Random.Range(0.9f, 1.1f);
            float volume = (float) Math.Pow(1 - normalisedDistance, 2);
             volume = Random.Range(0.9f, 1f) * volume;
            _instance.GunSource.volume = volume;
            switch (type)
            {
                case WeaponType.Pistol:
                    break;
                case WeaponType.Rifle:
                    _instance.GunSource.PlayOneShot(_instance.RifleFire[Random.Range(0, _instance.RifleFire.Length)]);
                    break;
                case WeaponType.Shotgun:
                    break;
                case WeaponType.SMG:
                    _instance.GunSource.PlayOneShot(_instance.SMGFire[Random.Range(0, _instance.SMGFire.Length)]);
                    break;
                case WeaponType.LMG:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public static void Cock(float duration)
        {
            _instance.GunSource.volume = 1;
            _instance.GunSource.PlayOneShot(_instance.BoltPull, duration);
        }
    }
}