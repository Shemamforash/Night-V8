using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;

public class StoryController : Menu, IInputListener
{
    private static string _text;
    public const float SkipAllTimerMax = 1f;
    private const float _timePerWord = 0.2f, MinAlpha = 0.25f;
    private List<string> _paragraphs;
    private TextMeshProUGUI _storyText;
    private static bool _goToCredits;
    private static bool _paused;
    private bool _skipParagraph;
    private float _heldCounter;
    private bool _skipAll;
    private CanvasGroup _skipCanvas;
    private AudioSource _audioSource;

    public override void Awake()
    {
        base.Awake();
        _storyText = GetComponent<TextMeshProUGUI>();
        _storyText.color = UiAppearanceController.InvisibleColour;
        _skipCanvas = GameObject.Find("Skip").GetComponent<CanvasGroup>();
        _audioSource = Camera.main.GetComponent<AudioSource>();
        _audioSource.volume = 0f;
        _audioSource.DOFade(1f, 1f).SetUpdate(UpdateType.Normal, true);
        _skipCanvas.alpha = MinAlpha;
        _paused = false;
    }

    public override void Enter()
    {
        InputHandler.RegisterInputListener(this);
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
        foreach (string paragraph in _paragraphs)
        {
            //fade in
            _storyText.text = paragraph + "\n\n    - <i>The Necromancer</i>";
            _storyText.color = UiAppearanceController.InvisibleColour;
            yield return _storyText.DOFade(1f, 1f).WaitForCompletion();

            //read
            float timeToRead = GetTimeToRead(paragraph);
            while (timeToRead > 0 && !_skipParagraph && !_skipAll)
            {
                if (!_paused) timeToRead -= Time.deltaTime;
                yield return null;
            }

            if (_skipAll) break;

            _skipParagraph = false;

            //fade out
            _storyText.color = Color.white;
            _audioSource.DOFade(0f, 1f).SetUpdate(UpdateType.Normal, true);
            yield return _storyText.DOFade(0f, 1f).WaitForCompletion();
        }

        End();
    }

    private void End()
    {
        SceneChanger.FadeInAudio();
        if (_goToCredits) SceneChanger.GoToCreditsScene();
        else SceneChanger.GoToGameScene();
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (axis != InputAxis.TakeItem || _paused || _skipAll || !isHeld) return;
        _heldCounter += Time.deltaTime;
        float normalised = _heldCounter / SkipAllTimerMax;
        if (normalised > 1) normalised = 1;
        normalised *= normalised;
        _skipCanvas.alpha = Mathf.Lerp(MinAlpha, 0.8f, normalised);
        if (_heldCounter < SkipAllTimerMax) return;
        _skipCanvas.alpha = 1f;
        _skipAll = true;
    }

    public void OnInputUp(InputAxis axis)
    {
        if (axis != InputAxis.TakeItem || _paused || _skipParagraph || _skipAll) return;
        _skipParagraph = true;
        _skipCanvas.DOFade(MinAlpha, 0.2f);
        _heldCounter = 0;
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
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