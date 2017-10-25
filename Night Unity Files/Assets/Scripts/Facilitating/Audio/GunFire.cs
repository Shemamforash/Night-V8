using UnityEngine;

namespace Facilitating.Audio
{
    public class GunFire : MonoBehaviour
    {
        private AudioSource GunSource;
        public AudioClip[] RifleFire;
        private static GunFire _instance;

        public void Awake()
        {
            _instance = this;
            GunSource = GetComponent<AudioSource>();
        }
        
        public static void Fire()
        {
            _instance.GunSource.pitch = Random.Range(0.9f, 1.1f);
            _instance.GunSource.volume = Random.Range(0.95f, 1f);
            _instance.GunSource.PlayOneShot(_instance.RifleFire[Random.Range(0, _instance.RifleFire.Length)]);
        }
    }
}