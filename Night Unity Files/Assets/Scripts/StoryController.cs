using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;

public class StoryController : Menu
{
    private static string _text;
    private const float _timePerWord = 0.3f, MinAlpha = 0.25f;
    private List<string> _paragraphs;
    private TextMeshProUGUI _storyText;
    private static bool _goToCredits;
    private static bool _paused;
    private bool _skipParagraph;
    private CanvasGroup _skipCanvas;
    private CloseButtonController _closeButton;
    private AudioSource _audioSource;
    private bool _canSkip;

    public override void Awake()
    {
        base.Awake();
        _storyText = GetComponent<TextMeshProUGUI>();
        _storyText.color = UiAppearanceController.InvisibleColour;
        _skipCanvas = GameObject.Find("Skip").GetComponent<CanvasGroup>();
        _closeButton = _skipCanvas.gameObject.FindChildWithName<CloseButtonController>("Close Button");
        _closeButton.SetCallback(Skip);
        _closeButton.SetOnClick(Skip);
        _closeButton.UseFireInput();
        _audioSource = Camera.main.GetComponent<AudioSource>();
        _audioSource.volume = 0f;
        _audioSource.DOFade(1f, 1f).SetUpdate(UpdateType.Normal, true);
        _skipCanvas.alpha = MinAlpha;
        _paused = false;
    }

    public override void Enter()
    {
        StartCoroutine(DisplayParagraph());
    }

    public static void ShowText(string text, bool goToCredits)
    {
        _text = text;
        _goToCredits = goToCredits;
        SceneChanger.GoToStoryScene();
    }

    public static float GetTimeToRead(string paragraph)
    {
        int wordCount = paragraph.Split(' ').Length;
        float timeToRead = _timePerWord * wordCount;
        return timeToRead;
    }

    private void SplitParagraphs()
    {
        _paragraphs = new List<string>();
        foreach (string paragraph in _text.Split(new[] {"\n"}, StringSplitOptions.None))
            _paragraphs.Add(paragraph);
    }

    private IEnumerator DisplayParagraph()
    {
        SplitParagraphs();
        Tweener skipTween = null;
        foreach (string paragraph in _paragraphs)
        {
            //fade in
            _storyText.text = paragraph + "\n\n    - <i>The Necromancer</i>";
            _storyText.color = UiAppearanceController.InvisibleColour;
            yield return _storyText.DOFade(1f, 1f).WaitForCompletion();

            skipTween?.Kill();
            skipTween = _skipCanvas.DOFade(0.5f, 1f);
            _closeButton.Enable();
            _canSkip = true;
            _skipCanvas.alpha = 1f;
            //read
            float timeToRead = GetTimeToRead(paragraph);
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