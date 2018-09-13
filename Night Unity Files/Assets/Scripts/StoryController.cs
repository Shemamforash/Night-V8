using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.Elements;
using TMPro;
using UnityEngine;

public class StoryController : MonoBehaviour, IInputListener
{
    private static string _text;
    private const float _timePerWord = 0.2f;
    private Queue<string> _paragraphs;
    private TextMeshProUGUI _storyText;
    private static string _nextMenu;
    private bool _skipParagraph;

    public void Awake()
    {
        _storyText = GetComponent<TextMeshProUGUI>();
    }

    public void Start()
    {
        _paragraphs = new Queue<string>();
        foreach (string paragraph in _text.Split(new[] {"\n"}, StringSplitOptions.None))
            _paragraphs.Enqueue(paragraph);
        StartCoroutine(DisplayParagraph());
        InputHandler.RegisterInputListener(this);
    }

    public static void ShowText(string text, string nextMenu)
    {
        _text = text;
        _nextMenu = nextMenu;
        SceneChanger.ChangeScene("Story");
    }
    

    private static float GetTimeToRead(string paragraph)
    {
        int wordCount = paragraph.Split(' ').Length;
        float timeToRead = _timePerWord * wordCount;
        return timeToRead;
    }

    private IEnumerator DisplayParagraph()
    {
        if (_paragraphs.Count == 0)
        {
            InputHandler.UnregisterInputListener(this);
            SceneChanger.ChangeScene(_nextMenu);
            yield break;
        }

        string currentParagraph = _paragraphs.Dequeue();
        float timeToRead = GetTimeToRead(currentParagraph);
        _storyText.text = currentParagraph + "\n\n    - <i>The Necromancer</i>";
        _storyText.color = UiAppearanceController.InvisibleColour;
        Tween fade = _storyText.DOColor(Color.white, 1f);
        yield return fade.WaitForCompletion();
        while (timeToRead > 0 && !_skipParagraph)
        {
            timeToRead -= Time.deltaTime;
            yield return null;
        }

        _skipParagraph = false;
        fade = _storyText.DOColor(UiAppearanceController.InvisibleColour, 1f);
        yield return fade.WaitForCompletion();
        StartCoroutine(DisplayParagraph());
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld || _skipParagraph) return;
        _skipParagraph = true;
    }

    public void OnInputUp(InputAxis axis)
    {
    }

    public void OnDoubleTap(InputAxis axis, float direction)
    {
    }
}