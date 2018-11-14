using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

namespace Game.Global
{
    public class AudioController : MonoBehaviour
    {
        private static AudioController _instance;
        private static AudioMixer _audioMixer;
        private static float _fogMuffle;

        [SerializeField] private AudioSource _windLight, _windMedium, _windHeavy;
        [SerializeField] private AudioSource _rainLight, _rainMedium, _rainHeavy;
        [SerializeField] private AudioSource _ambient;

        [SerializeField] private AudioSource _night;

        public void Awake()
        {
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


            DontDestroyOnLoad(this);
        }

        private static void Fade(float to, float duration, AudioSource source) => source.DOFade(to, duration).SetUpdate(UpdateType.Normal, true);

        public static void SetAmbientVolume(float volume) => _instance._ambient.volume = volume;

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