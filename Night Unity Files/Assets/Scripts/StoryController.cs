using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZCameraShake;
using Game.Exploration.Environment;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class StoryController : Menu
{
    private const float _timePerWord = 0.3f;
    private TextMeshProUGUI _storyText;
    private static bool _goToCredits;
    private static bool _paused;
    private bool _skipParagraph;
    private CanvasGroup _skipCanvas;
    private CloseButtonController _closeButton;
    private AudioSource _audioSource;
    private bool _canSkip;
    private ParticleSystem _lightningSystem;
    private CameraShaker _shaker;
    private float _waitTime;
    private static List<JournalEntry> _journalEntries;

    public override void Awake()
    {
        base.Awake();
        _storyText = GetComponent<TextMeshProUGUI>();
        _storyText.color = UiAppearanceController.InvisibleColour;
        _skipCanvas = GameObject.Find("Skip").GetComponent<CanvasGroup>();
        _closeButton = _skipCanvas.GetComponent<CloseButtonController>();
        _closeButton.SetCallback(Skip);
        _closeButton.SetOnClick(Skip);
        Debug.Log("using fire input");
        _closeButton.UseFireInput();
        _audioSource = Camera.main.GetComponent<AudioSource>();
        _audioSource.volume = 0f;
        _audioSource.DOFade(1f, 1f).SetUpdate(UpdateType.Normal, true);
        _skipCanvas.alpha = 0f;
        _paused = false;
        if (!_goToCredits) return;
        _waitTime = Random.Range(2f, 4f);
        _lightningSystem = GameObject.Find("Lightning").GetComponent<ParticleSystem>();
        _shaker = GameObject.Find("Shaker").GetComponent<CameraShaker>();
        _lightningSystem.Play();
    }

    public void Update()
    {
        if (!_goToCredits) return;
        _waitTime -= Time.deltaTime;
        if (_waitTime > 0f) return;
        float magnitude = Random.Range(5, 10);
        float roughness = Random.Range(5, 10);
        float inDuration = Random.Range(0.5f, 2f);
        float outDuration = Random.Range(0.5f, 2f);
        _shaker.ShakeOnce(magnitude, roughness, inDuration, outDuration);
        float totalDuration = inDuration + outDuration;
        _waitTime = Random.Range(totalDuration * 2f, totalDuration * 4f);
    }

    public override void Enter()
    {
        StartCoroutine(DisplayParagraph());
    }

    public static void Show()
    {
        _journalEntries = JournalEntry.GetStoryText();
        _goToCredits = EnvironmentManager.CurrentEnvironmentType() == EnvironmentType.End;
        SceneChanger.GoToStoryScene();
    }

    public static float GetTimeToRead(string paragraph)
    {
        int wordCount = paragraph.Split(' ').Length;
        float timeToRead = _timePerWord * wordCount;
        return timeToRead;
    }

    private IEnumerator DisplayParagraph()
    {
        Tweener skipTween = null;
        foreach (JournalEntry entry in _journalEntries)
        {
            //fade in
            _storyText.text = entry.Text + "\n\n    - <i>The Necromancer</i>";
            _storyText.color = UiAppearanceController.InvisibleColour;
            yield return _storyText.DOFade(1f, 1f).WaitForCompletion();

            skipTween?.Kill();
            skipTween = _skipCanvas.DOFade(0.5f, 1f);
            _closeButton.Enable();
            _canSkip = true;
            _skipCanvas.alpha = 1f;
            //read
            float timeToRead = GetTimeToRead(entry.Text);
            while (timeToRead > 0 && !_skipParagraph)
            {
                if (!_paused) timeToRead -= Time.deltaTime;
                yield return null;
            }

            _canSkip = false;
            _skipParagraph = false;
            _closeButton.Disable();
            skipTween?.Kill();
            skipTween = _skipCanvas.DOFade(0, 1f);

            //fade out
            _storyText.color = Color.white;
            yield return _storyText.DOFade(0f, 1f).WaitForCompletion();
        }

        _audioSource.DOFade(0f, 0.5f).SetUpdate(UpdateType.Normal, true);
        End();
    }

    private void End()
    {
        _closeButton.Disable();
        SceneChanger.FadeInAudio();
        if (_goToCredits) SceneChanger.GoToCreditsScene();
        else SceneChanger.GoToGameScene();
    }

    private void Skip()
    {
        if (_skipParagraph || !_canSkip) return;
        _skipParagraph = true;
        _skipCanvas.alpha = 0.8f;
    }

    public static void Pause()
    {
        _paused = true;
    }

    public static void Unpause()
    {
        _paused = false;
    }
}