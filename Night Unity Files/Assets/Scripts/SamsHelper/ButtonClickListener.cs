using Facilitating.Persistence;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class ButtonClickListener : MonoBehaviour
{
    private static AudioClip _buttonSelectClip;
    private static AudioSource _audioSource;
    private static bool _suppressClick;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.outputAudioMixerGroup = Resources.Load<AudioMixer>("AudioMixer/Master").FindMatchingGroups("Modified")[0];
    }

    public static void Click()
    {
        if (_buttonSelectClip == null) _buttonSelectClip = Helper.LoadFileFromAssetBundle<AudioClip>("misc", "Button Click");
        if (_suppressClick)
        {
            _suppressClick = false;
            return;
        }

        if (_audioSource == null) return;
        _audioSource.pitch = Random.Range(0.9f, 1f);
        _audioSource.PlayOneShot(_buttonSelectClip);
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