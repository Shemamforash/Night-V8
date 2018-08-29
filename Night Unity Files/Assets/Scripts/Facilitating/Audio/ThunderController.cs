using System.Collections;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Weather;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.UI;

namespace Facilitating.Audio
{
    public class ThunderController : MonoBehaviour
    {
        private const float LightningDuration = 0.15f;
        private static ThunderController _instance;
        
        private AudioSource thunderSource;
        private float lightningTimer;
        private bool _waitingForThunder;

        public AudioClip[] thunderSounds;
        public Image lightningImage;

        public void Awake()
        {
            thunderSource = GetComponent<AudioSource>();
            _instance = this;
        }

        private IEnumerator LightningFlash()
        {
            thunderSource.volume = Random.Range(0.9f, 1f);
            thunderSource.pitch = Random.Range(0.8f, 1f);
            thunderSource.PlayOneShot(thunderSounds.RandomElement(), Random.Range(0.6f, 1f));
            lightningTimer = LightningDuration;
            while (lightningTimer > 0f)
            {
                lightningTimer += Random.Range(-0.01f, 0.005f);
                if (lightningTimer < 0f) lightningTimer = 0f;
                float opacity = 1 / LightningDuration * lightningTimer;
                lightningImage.color = new Color(1f, 1f, 1f, opacity);
                yield return null;
            }
        }

        private static void Strike()
        {
            _instance.StartCoroutine(_instance.LightningFlash());
            if (!CombatManager.InCombat()) return;
            Vector2 firePosition = PathingGrid.GetCellNearMe(PlayerCombat.Instance.CurrentCell(), 12f).Position;
            FireBehaviour.Create(firePosition, 1f);
        }

        public void Update()
        {
            UpdateThunder();
        }

        private void UpdateThunder()
        {
//            if (WeatherManager.CurrentWeather().Thunder == 0) return;
            if (_waitingForThunder) return;
            StartCoroutine(ThunderStrike());
        }

        private IEnumerator ThunderStrike()
        {
            _waitingForThunder = true;
            float pause = 4 - WeatherManager.CurrentWeather().Thunder;
            float pauseMultiplier = CombatManager.InCombat() ? 15f : 5f;
            pause *= pauseMultiplier;
            pause = Random.Range(0.75f * pause, 1.25f * pause);
            while (pause > 0f)
            {
                pause -= Time.deltaTime;
                yield return null;
            }

            Strike();
            _waitingForThunder = false;
        }
    }
}