using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Combat.Enemies.Nightmares;
using Game.Combat.Generation;
using Game.Combat.Misc;
using Game.Combat.Player;
using Game.Exploration.Regions;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Game.Global
{
    public class AudioController : MonoBehaviour
    {
        private static float _ambientVolume;
        private static AudioController _instance;
        private static AudioMixer _audioMixer;
        private static float _fogMuffle;
        private float _combatTargetVolume;
        private const float ThresholdCombatMusicDistance = 10f;
        private const float MusicFadeDuration = 5f;
        private float _timeNearEnemies;

        [SerializeField] private AudioSource _windLight, _windMedium, _windHeavy;
        [SerializeField] private AudioSource _rainLight, _rainMedium, _rainHeavy;
        [SerializeField] private AudioSource _ambient;
        [SerializeField] private AudioSource _layer1, _layer2;
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

            float layer1StartTime = Random.Range(0f, AudioClips.AbandonedLands.length);
            float layer2StartTime = Random.Range(0f, AudioClips.GodsAreDead.length);
            _layer1.clip = AudioClips.AbandonedLands;
            _layer2.clip = AudioClips.GodsAreDead;

            SetInitialVolume(_layer1, 0, layer1StartTime);
            SetInitialVolume(_layer2, 0, layer2StartTime);

            DontDestroyOnLoad(this);
        }

        private void SetInitialVolume(AudioSource source, float volume, float startTime)
        {
            source.volume = volume;
            source.time = startTime;
            source.Play();
        }

        private int GetEnemiesInRange()
        {
            if (PlayerCombat.Instance == null) return -1;
            Vector2 playerPosition = PlayerCombat.Position();
            List<CanTakeDamage> enemies = CombatManager.Enemies();
            if (enemies.Count == 0) return -1;
            int enemiesInRange = enemies.Count(e => e.transform.Distance(playerPosition) <= ThresholdCombatMusicDistance && !(e is AnimalBehaviour));
            return enemiesInRange;
        }

        private void UpdateTimeNearEnemies()
        {
            int enemiesInRange = GetEnemiesInRange();
            float timeChange = Time.deltaTime;
            if (enemiesInRange == -1) timeChange *= -MusicFadeDuration;
            if (enemiesInRange == 0) timeChange *= -1;
            _timeNearEnemies = Mathf.Clamp(_timeNearEnemies + timeChange, 0, 5);
        }


        private float _minAmbientVolume, _maxCombatVolume;
        private bool _useAlternateCombatMusic;

        private void UpdateMinMaxVolumes(bool isCombatScene)
        {
            if (isCombatScene)
            {
                Region currentRegion = CombatManager.GetCurrentRegion();
                if (currentRegion.IsDynamic())
                {
                    _minAmbientVolume = 0.25f;
                    _maxCombatVolume = 0.75f;
                }
                else
                {
                    _minAmbientVolume = 0f;
                    _maxCombatVolume = 1f;
                }

                _useAlternateCombatMusic = currentRegion.GetRegionType() == RegionType.Tomb;
            }
        }

        private void UpdateInGameVolumes()
        {
            float ambientVolumeModifier = 1 - _combatTargetVolume;
            if (ambientVolumeModifier < _minAmbientVolume) ambientVolumeModifier = _minAmbientVolume;
            _ambient.volume = _ambientVolume * ambientVolumeModifier;
            if (_combatTargetVolume > _maxCombatVolume) _combatTargetVolume = _maxCombatVolume;
        }

        private void UpdateOutOfGameVolumes()
        {
            _combatTargetVolume = 0;
            _ambientVolume -= Time.deltaTime;
            if (_ambientVolume < 0) _ambientVolume = 0f;
            _ambient.volume = _ambientVolume;
        }

        public void Update()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            bool isGameScene = currentScene == "Game";
            bool isCombatScene = currentScene == "Combat";
            UpdateMinMaxVolumes(isCombatScene);
            UpdateTimeNearEnemies();

            _combatTargetVolume = _timeNearEnemies / 5f;

            if (!isGameScene && !isCombatScene) UpdateOutOfGameVolumes();
            else UpdateInGameVolumes();

            float layer1TargetVolume = _useAlternateCombatMusic ? 0f : _combatTargetVolume;
            float layer2TargetVolume = _useAlternateCombatMusic ? _combatTargetVolume : 0f;
            UpdateLayerVolume(layer1TargetVolume, _layer1);
            UpdateLayerVolume(layer2TargetVolume, _layer2);
        }

        private void UpdateLayerVolume(float targetVolume, AudioSource layer)
        {
            float layerDifference = targetVolume - layer.volume;
            float incrementAmount = 1f;
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

        private static Tweener _weatherTween;

        public static void FadeWeatherOut()
        {
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _weatherTween?.Kill();
            _weatherTween = _audioMixer.DOSetFloat("WeatherVolume", -80, 1).SetUpdate(UpdateType.Normal, true);
        }

        public static void FadeWeatherIn()
        {
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _weatherTween?.Kill();
            _weatherTween = _audioMixer.DOSetFloat("WeatherVolume", 0, 1).SetUpdate(UpdateType.Normal, true);
        }

        private static Tweener _musicTween, _combatTween;

        public static void FadeInMusicMuffle()
        {
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _musicTween?.Kill();
            _musicTween = _audioMixer.DOSetFloat("MusicLowPassCutoff", 750, 0.5f).SetUpdate(UpdateType.Normal, true);
        }

        public static void FadeOutMusicMuffle()
        {
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _musicTween?.Kill();
            _musicTween = _audioMixer.DOSetFloat("MusicLowPassCutoff", 22000, 0.5f).SetUpdate(UpdateType.Normal, true);
        }

        public static void FadeInCombat()
        {
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _combatTween?.Kill();
            _combatTween = _audioMixer.DOSetFloat("CombatVolume", 0, 0.5f).SetUpdate(UpdateType.Normal, true);
        }

        public static void FadeOutCombat()
        {
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _combatTween?.Kill();
            _combatTween = _audioMixer.DOSetFloat("CombatVolume", -80f, 0.5f).SetUpdate(UpdateType.Normal, true);

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

        private static Tweener _muffleTween;

        public static void FadeInGlobalMuffle()
        {
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _muffleTween?.Kill();
            _muffleTween = _audioMixer.DOSetFloat("Muffle", 750, 1f).SetUpdate(UpdateType.Normal, true);
        }

        public static void FadeOutGlobalMuffle()
        {
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _muffleTween?.Kill();
            _muffleTween = _audioMixer.DOSetFloat("Muffle", 22000, 1f).SetUpdate(UpdateType.Normal, true);
        }

        public static void SetGlobalMuffle(float value)
        {
            if (_audioMixer == null) _audioMixer = Resources.Load<AudioMixer>("AudioMixer/Master");
            _audioMixer.SetFloat("SecondaryMuffle", value);
        }

        private static float NormalisedVolumeToAttenuation(float volume)
        {
            if (volume == 0) volume = 0.001f;
            return Mathf.Log(volume) * 20f;
        }
    }
}