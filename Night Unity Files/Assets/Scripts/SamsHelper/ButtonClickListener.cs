using Facilitating.Persistence;
using Game.Global;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class ButtonClickListener : MonoBehaviour
{
    private static AudioSource _audioSource;
    private static bool _suppressClick;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.outputAudioMixerGroup = Resources.Load<AudioMixer>("AudioMixer/Master").FindMatchingGroups("Modified")[0];
    }

    public static void Click()
    {
        if (_suppressClick)
        {
            _suppressClick = false;
            return;
        }

        if (_audioSource == null) return;
        _audioSource.pitch = Random.Range(0.9f, 1f);
        _audioSource.volume = 0.5f;
        _audioSource.PlayOneShot(AudioClips.ButtonSelectClip);
    }

    public static void SuppressClick()
    {
        _suppressClick = true;
    }

    public void OnApplicationQuit()
    {
        if (SceneManager.GetActiveScene().name == "Menu") return;
        SaveController.QuickSave();
    }
}