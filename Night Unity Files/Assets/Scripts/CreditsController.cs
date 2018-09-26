using System.Collections.Generic;
using DG.Tweening;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsController : Menu, IInputListener
{
    private readonly List<TextMeshProUGUI> _texts = new List<TextMeshProUGUI>();
    private static Sequence _creditsSequence;
    private static bool _paused;

    public override void Awake()
    {
        base.Awake();
        _paused = false;
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; ++i)
        {
            _texts.Add(transform.GetChild(i).GetComponent<TextMeshProUGUI>());
            _texts[i].color = UiAppearanceController.InvisibleColour;
        }
    }

    public override void Enter()
    {
        base.Enter();
        InputHandler.SetCurrentListener(this);
        FadeInText();
    }

    private void FadeInText()
    {
        _creditsSequence = DOTween.Sequence();
        foreach (TextMeshProUGUI text in _texts)
        {
            _creditsSequence.Append(text.DOColor(Color.white, 1f));
            float timeToRead = StoryController.GetTimeToRead(text.text);
            if (timeToRead < 5f) timeToRead = 5f;
            _creditsSequence.AppendInterval(timeToRead);
            _creditsSequence.Append(text.DOColor(UiAppearanceController.InvisibleColour, 1f));
        }

        _creditsSequence.AppendCallback(SceneChanger.GoToMainMenuScene);
    }

    public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
    {
        if (isHeld || axis == InputAxis.Menu || _paused) return;
        _creditsSequence.Complete(true);
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
        _creditsSequence.Pause();
    }

    public static void Unpause()
    {
        _paused = false;
        _creditsSequence.Play();
    }
}