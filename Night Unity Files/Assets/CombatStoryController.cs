using DG.Tweening;
using Game.Combat.Generation;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using UnityEngine;

public class CombatStoryController : MonoBehaviour
{
    private CanvasGroup _storyCanvas;
    private EnhancedText _titleText, _contentText;
    private CloseButtonController _closeButton;
    private static JournalEntry _journalHere;
    public static bool ShouldShow;

    public void Awake()
    {
        _storyCanvas = gameObject.FindChildWithName<CanvasGroup>("Foreground");
        _closeButton = _storyCanvas.gameObject.FindChildWithName<CloseButtonController>("Close Button");
        _closeButton.SetCallback(FadeOut);
        _closeButton.SetOnClick(FadeOut);
        _closeButton.UseAcceptInput();
        _titleText = _storyCanvas.gameObject.FindChildWithName<EnhancedText>("Title");
        _contentText = _storyCanvas.gameObject.FindChildWithName<EnhancedText>("Content");
        _titleText.SetText(_journalHere.Title);
        _contentText.SetText(_journalHere.Text);
        _journalHere.Unlock();
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_storyCanvas.DOFade(1f, 1f));
        sequence.AppendCallback(_closeButton.Enable);
    }

    private void FadeOut()
    {
        _closeButton.Disable();
        _storyCanvas.DOFade(0f, 5f);
        ScreenFaderController.FadeIn(5f);
        SceneChanger.GoToCombatScene();
    }

    public static void TryEnter()
    {
        if (ShouldShow)
        {
            SceneChanger.GoToCombatStoryScene();
            _journalHere = JournalEntry.GetStoryEntry();
            ShouldShow = false;
        }
        else
        {
            SceneChanger.GoToCombatScene();
        }
    }
}