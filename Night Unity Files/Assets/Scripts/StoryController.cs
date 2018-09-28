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
    private const float _timePerWord = 0.2f;
    private List<string> _paragraphs;
    private TextMeshProUGUI _storyText;
    private static bool _goToCredits;
    private static bool _paused;
    private bool _skipParagraph;

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
            while (timeToRead > 0 && !_skipParagraph)
            {
                if (!_paused) timeToRead -= Time.deltaTime;
                yield return null;
            }

            _skipParagraph = false;

            //fade out
            _storyText.color = Color.white;
            yield return _storyText.DOFade(0f, 1f).WaitForCompletion();
        }

        End();
    }

    private void End()
    {
        if (_goToCredits) SceneChanger.GoToCreditsScene();
        else SceneChanger.GoToGameScene();
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld || _paused || axis != InputAxis.TakeItem || _skipParagraph) return;
        _skipParagraph = true;
    }

    public void OnInputUp(InputAxis axis)
    {
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