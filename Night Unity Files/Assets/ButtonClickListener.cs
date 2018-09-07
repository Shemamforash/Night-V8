using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class ButtonClickListener : MonoBehaviour
{
    private static AudioClip[] _buttonSelectClips;
    private static AudioSource _audioSource;
    private static bool _suppressClick;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.outputAudioMixerGroup = Resources.Load<AudioMixer>("AudioMixer/Master").FindMatchingGroups("Modified")[0];
    }

    public static void Click()
    {
        if (_buttonSelectClips == null) _buttonSelectClips = Resources.LoadAll<AudioClip>("Sounds/Button Clicks");
        if (_suppressClick)
        {
            _suppressClick = false;
            return;
        }

        _audioSource.pitch = Random.Range(0.85f, 1f);
        _audioSource.PlayOneShot(_buttonSelectClips.RandomElement());
    }

    public static void SuppressClick()
    {
        _suppressClick = true;
    }
}