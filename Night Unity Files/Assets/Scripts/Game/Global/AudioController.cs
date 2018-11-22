using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Global
{
    public class AudioController : MonoBehaviour
    {
        private static float _ambientVolume;
        private static AudioController _instance;
        private static AudioMixer _audioMixer;
        private static float _fogMuffle, _startTime;
        private float _layer1TargetVolume, _layer2TargetVolume, _layer3TargetVolume, _layer4TargetVolume;
        private float _layer1VolumeGain = 2f, _layer2VolumeGain = 1f, _layer3VolumeGain = 0.5f, _layer4VolumeGain = 0.25f;
        private const float ThresholdCombatMusicDistance = 10f;
        private const float MusicFadeDuration = 5f;
        private float _timeNearEnemies;

        [SerializeField] private AudioSource _windLight, _windMedium, _windHeavy;
        [SerializeField] private AudioSource _rainLight, _rainMedium, _rainHeavy;
        [SerializeField] private AudioSource _ambient;
        [SerializeField] private AudioSource _layer1, _layer2, _layer3, _layer4;
        [SerializeField] private AudioSource _night;

        public void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            _windLight.clip = AudioClips.LightWind;
            _windLight.Play();
            _windMedium.clip = AudioClips.MediumWind;
            _windMedium.Play();
            _windHeavy.clip = AudioClips.HeavyWind;
            _windHeavy.Play();

            _rainLight.clip = AudioClips.LightRain;
            _rainLight.Play();
            _rainMedium.clip = AudioClips.MediumRain;
            _rainMedium.Play();
            _rainHeavy.clip = AudioClips.HeavyRain;
            _rainHeavy.Play();

            _ambient.clip = AudioClips.Ambient;
            _ambient.time = Random.Range(0f, _ambient.clip.length);
            _ambient.Play();
            _night.clip = AudioClips.Night;
            _night.Play();

            _startTime = Random.Range(0f, AudioClips.SimmavA.length);
            _layer1.clip = AudioClips.SimmavA;
            _layer2.clip = AudioClips.SimmavB;
            _layer3.clip = AudioClips.SimmavC;
            _layer4.clip = AudioClips.SimmavD;

            SetInitialVolume(_layer1, 0);
            SetInitialVolume(_layer2, 0);
            SetInitialVolume(_layer3, 0);
            SetInitialVolume(_layer4, 0);

            DontDestroyOnLoad(this);
        }

        private void SetInitialVolume(AudioSource source, float volume)
        {
            source.volume = volume;
            source.time = _startTime;
            source.Play();
        }

        private int GetEnemiesInRange()
        {
            if (PlayerCombat.Instance == null) return -1;
            Vector2 playerPosition = PlayerCombat.Instance.transform.position;
            List<CanTakeDamage> enemies = CombatManager.Enemies();
            if (enemies.Count == 0) return -1;
            int enemiesInRange = enemies.Count(e => e.transform.Distance(playerPosition) <= ThresholdCombatMusicDistance);
            return enemiesInRange;
        }

        public void Update()
        {
            float ambientVolumeModifier = 1;

            int enemiesInRange = GetEnemiesInRange();
            float timeChange = Time.deltaTime;
            if (enemiesInRange == -1) timeChange *= -MusicFadeDuration;
            if (enemiesInRange == 0) timeChange *= -1;
            
            _timeNearEnemies = Mathf.Clamp(_timeNearEnemies + timeChange, 0, 25);
            ambientVolumeModifier = 1 - _timeNearEnemies;

            if (ambientVolumeModifier < 1)
            {
                ambientVolumeModifier += Time.deltaTime;
                if (ambientVolumeModifier > 1) ambientVolumeModifier = 1;
            }

            _layer4TargetVolume = 0.1f * (_timeNearEnemies - 15f);
            _layer3TargetVolume = 0.1f * (_timeNearEnemies - 10f);
            _layer2TargetVolume = 0.1f * (_timeNearEnemies - 5f);
            _layer1TargetVolume = 0.1f * _timeNearEnemies;

            _layer1TargetVolume = Mathf.Clamp(_layer1TargetVolume, 0f, 1f);
            _layer2TargetVolume = Mathf.Clamp(_layer2TargetVolume, 0f, 1f);
            _layer3TargetVolume = Mathf.Clamp(_layer3TargetVolume, 0f, 1f);
            _layer4TargetVolume = Mathf.Clamp(_layer4TargetVolume, 0f, 1f);

            UpdateLayerVolume(_layer1TargetVolume, _layer1, _layer1VolumeGain);
            UpdateLayerVolume(_layer2TargetVolume, _layer2, _layer2VolumeGain);
            UpdateLayerVolume(_layer3TargetVolume, _layer3, _layer3VolumeGain);
            UpdateLayerVolume(_layer4TargetVolume, _layer4, _layer4VolumeGain);

            _ambient.volume = _ambientVolume * ambientVolumeModifier;
        }

        private void UpdateLayerVolume(float targetVolume, AudioSource layer, float incrementAmount)
        {
            float layerDifference = targetVolume - layer.volume;
            if (incrementAmount > Mathf.Abs(layerDifference)) incrementAmount = Mathf.Abs(layerDifference);
            if (layerDifference < 0) incrementAmount = -incrementAmount;
            layer.volume += incrementAmount * Time.deltaTime;
        }

        private static void Fade(float to, float duration, AudioSource source) => source.DOFade(to, duration).SetUpdate(UpdateType.Normal, true);

        public static void SetAmbientVolume(float volume) => _ambientVolume = volume;

        public static void SetNightVolume(float volume) => _instance._night.volume = volume;

        public static void FadeWindLight(float to, float duration = 1f) => Fade(to * 0.5f, duration, _instance._windLight);

        public static void FadeWindMedium(float to, float duration = 1f) => Fade(to * 0.5f, duration, _instance._windMedium);

        public static void FadeWindHeavy(float to, float duration = 1f) => Fade(to * 0.5f, duration, _instance._windHeavy);

        public static void FadeRainLight(float to, float duration = 1f) => Fade(to * 0.5f, duration, _instance._rainLight);

        public static void FadeRainMedium(float to, float duration = 1f) => Fade(to * 0.5f, duration, _instance._rainMedium);

        public static void FadeRainHeavy(float to, float duration = 1f) => Fade(to * 0.5f, duration, _instance._rainHeavy);

        public static void FadeFog(float to, float duration = 1f)
        {
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _audioMixer.DOSetFloat("WeatherWetMix", to, duration).SetUpdate(UpdateType.Normal, true);
            _audioMixer.DOSetFloat("WeatherCutoffFreq", 22000 - to * 20000f, duration).SetUpdate(UpdateType.Normal, true);
        }

        public static void FadeInMuffle()
        {
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _audioMixer.DOSetFloat("MusicLowPassCutoff", 750, 0.5f).SetUpdate(UpdateType.Normal, true);
        }

        public static void FadeOutMuffle()
        {
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _audioMixer.DOSetFloat("MusicLowPassCutoff", 22000, 0.5f).SetUpdate(UpdateType.Normal, true);
        }

        public static void SetMasterVolume(float volume)
        {
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _audioMixer.SetFloat("Master", NormalisedVolumeToAttenuation(volume));
        }

        public static void SetModifiedVolume(float volume)
        {
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _audioMixer.SetFloat("Modified", NormalisedVolumeToAttenuation(volume));
        }

        private static float NormalisedVolumeToAttenuation(float volume)
        {
            if (volume == 0) volume = 0.001f;
            return Mathf.Log(volume) * 20f;
        }
    }
}