using DG.Tweening;
using Game.Global;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class CreditsController : Menu
{
    private AudioSource _creditsAudio;
    private static Sequence _creditsSequence;

    protected override void Awake()
    {
        base.Awake();
        _creditsAudio = GetComponent<AudioSource>();
        FadeInText();
    }

    private void FadeInText()
    {
        _creditsSequence = DOTween.Sequence();
        _creditsSequence.AppendInterval(10f);
        _creditsSequence.Append(_creditsAudio.DOFade(0f, 1f));
        _creditsSequence.AppendCallback(SceneChanger.GoToMainMenuScene);
    }

    public static void Pause()
    {
        _creditsSequence.Pause();
    }

    public static void Unpause()
    {
        _creditsSequence.Play();
    }
}