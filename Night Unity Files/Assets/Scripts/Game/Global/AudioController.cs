using DG.Tweening;
using Game.Characters;
using Game.Exploration.Regions;
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
        private float _minAmbientVolume;

        [SerializeField] private AudioSource _windLight, _windMedium, _windHeavy;
        [SerializeField] private AudioSource _rainLight, _rainMedium, _rainHeavy;
        [SerializeField] private AudioSource _hail;
        [SerializeField] private AudioSource _ambient;
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
            _hail.clip = AudioClips.Hail;
            _hail.Play();
            _ambientVolume = 1;

            DontDestroyOnLoad(this);
        }

        private void UpdateMinMaxVolumes(bool isCombatScene)
        {
            if (!isCombatScene) return;
            Region currentRegion = CharacterManager.CurrentRegion();
            _minAmbientVolume = currentRegion.IsDynamic() ? 0.25f : 0f;
        }

        private void UpdateInGameVolumes()
        {
            float ambientVolumeModifier = 1 - CombatAudioController.GetTargetVolume();
            if (EndGameAudioController.Active()) ambientVolumeModifier = 0f;
            if (ambientVolumeModifier < _minAmbientVolume) ambientVolumeModifier = _minAmbientVolume;
            _ambient.volume = _ambientVolume * ambientVolumeModifier;
        }

        private void UpdateOutOfGameVolumes()
        {
            _ambientVolume -= Time.deltaTime;
            if (_ambientVolume < 0) _ambientVolume = 0f;
            _ambient.volume = _ambientVolume;
        }

        public void Update()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            UpdateMinMaxVolumes(currentScene == "Combat");
            bool isInGame = currentScene == "Game" || currentScene == "Combat" || currentScene == "Combat Story";
            if (isInGame) UpdateInGameVolumes();
            else UpdateOutOfGameVolumes();
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

        public static void FadeHail(float to, float duration = 1f) => Fade(to * 0.5f, duration, _instance._hail);

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