using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class ButtonClickListener : MonoBehaviour
{
    private static AudioLowPassFilter _lpf;
    private static AudioSource _audioSource;
    private static bool _suppressClick;

    private void Awake()
    {
        _lpf = GetComponent<AudioLowPassFilter>();
        _audioSource = GetComponent<AudioSource>();
        _audioSource.outputAudioMixerGroup = Resources.Load<AudioMixer>("AudioMixer/Master").FindMatchingGroups("Modified")[0];
    }

    public static void Click(bool muffle = false)
    {
        if (_suppressClick)
        {
            _suppressClick = false;
            return;
        }

        Assert.IsNotNull(_audioSource);
        _audioSource.pitch = Random.Range(0.9f, 1f);
        _audioSource.volume = 0.15f;
        _lpf.cutoffFrequency = muffle ? 3000 : 20000;
        _audioSource.Play();
    }

    public static void SuppressClick()
    {
        _suppressClick = true;
    }
}