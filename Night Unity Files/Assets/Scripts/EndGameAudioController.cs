using DG.Tweening;
using Game.Global;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameAudioController : MonoBehaviour
{
    private AudioSource _audioSource;
    private static EndGameAudioController _instance;
    private static bool _active;

    public void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSource.clip = AudioClips.AtTheEnd;
        _audioSource.Play();
        _active = true;
        DontDestroyOnLoad(this);
        Stop();
        _instance = this;
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode loadMode)
    {
        if (scene.name == "Story") return;
        Stop();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoad;
    }

    private static void Stop()
    {
        if (_instance == null) return;
        Sequence sequence = DOTween.Sequence();
        EndGameAudioController endGameAudio = _instance;
        sequence.Append(endGameAudio._audioSource.DOFade(0f, 1f));
        sequence.AppendCallback(() => Destroy(endGameAudio.gameObject));
        _active = false;
    }

    public static bool Active() => _active;
}