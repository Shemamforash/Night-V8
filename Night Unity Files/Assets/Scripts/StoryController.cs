using System;
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
    private const float _timePerWord = 0.2f;
    private List<string> _paragraphs;
    private TextMeshProUGUI _storyText;
    private static bool _goToCredits;
    private static bool _paused;
    private static Sequence _storySequence;

    public override void Awake()
    {
        base.Awake();
        _storyText = GetComponent<TextMeshProUGUI>();
        _storyText.color = UiAppearanceController.InvisibleColour;
        _paused = false;
    }

    public override void Enter()
    {
        InputHandler.RegisterInputListener(this);
        DisplayParagraph();
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

    private void FadeIn(string paragraph)
    {
        _storySequence.AppendCallback(() =>
        {
            _storyText.text = paragraph + "\n\n    - <i>The Necromancer</i>";
            _storyText.color = UiAppearanceController.InvisibleColour;
        });
        _storySequence.Append(_storyText.DOColor(Color.white, 1f));
    }

    private void ReadParagraph(string paragraph)
    {
        float timeToRead = GetTimeToRead(paragraph);
        _storySequence.AppendInterval(timeToRead);
    }

    private void FadeOut()
    {
        _storySequence.AppendCallback(() => _storyText.color = Color.white);
        _storySequence.Append(_storyText.DOColor(UiAppearanceController.InvisibleColour, 1f));
    }

    private void SplitParagraphs()
    {
        _paragraphs = new List<string>();
        foreach (string paragraph in _text.Split(new[] {"\n"}, StringSplitOptions.None))
            _paragraphs.Add(paragraph);
    }

    private void DisplayParagraph()
    {
        SplitParagraphs();
        _storySequence = DOTween.Sequence();
        foreach (string paragraph in _paragraphs)
        {
            FadeIn(paragraph);
            ReadParagraph(paragraph);
            FadeOut();
        }
        _storySequence.AppendCallback(End);
    }

    private void End()
    {
        if (_goToCredits) SceneChanger.GoToCreditsScene();
        else SceneChanger.GoToGameScene();
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld || _paused || axis == InputAxis.Menu) return;
        _storySequence.Complete(true);
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }

    public static void Pause()
    {
        _storySequence.Pause();
        _paused = true;
    }

    public static void Unpause()
    {
        _storySequence.Play();
        _paused = false;
    }
}