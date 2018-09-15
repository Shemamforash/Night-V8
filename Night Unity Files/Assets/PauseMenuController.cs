using DG.Tweening;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuController : Menu
{
    private static bool _open;
    private static string _lastMenu;
    private CanvasGroup _pausedCanvas, _optionsCanvas, _controlsCanvas;
    private EnhancedButton _pausedButton, _controlsButton;
    private Slider _volumeSlider;
    private const float FadeDuration = 0.5f;

    public override void Awake()
    {
        base.Awake();
        _pausedCanvas = gameObject.FindChildWithName<CanvasGroup>("Paused");
        _optionsCanvas = gameObject.FindChildWithName<CanvasGroup>("Options Canvas");
        _controlsCanvas = gameObject.FindChildWithName<CanvasGroup>("Controls Canvas");

        _pausedButton = gameObject.FindChildWithName<EnhancedButton>("Resume");
        _volumeSlider = gameObject.FindChildWithName<Slider>("Volume");
        _controlsButton = _controlsCanvas.gameObject.FindChildWithName<EnhancedButton>("Back");
    }

    public override void Enter()
    {
        base.Enter();
        _lastMenu = MenuStateMachine.CurrentMenu().gameObject.name;
        _optionsCanvas.alpha = 1;
        _open = true;
    }

    private static void Hide()
    {
        MenuStateMachine.ShowMenu(_lastMenu);
        _open = false;
    }

    public void ShowOptions()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(_controlsCanvas.DOFade(0, FadeDuration));
        seq.Insert(0, _pausedCanvas.DOFade(0, FadeDuration));
        seq.Append(_optionsCanvas.DOFade(1, FadeDuration));
        _volumeSlider.Select();
    }

    public void ShowControls()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(_optionsCanvas.DOFade(0, FadeDuration));
        seq.Insert(0, _pausedCanvas.DOFade(0, FadeDuration));
        seq.Append(_controlsCanvas.DOFade(1, FadeDuration));
        _controlsButton.Select();
    }

    public void ShowPauseMenu()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(_optionsCanvas.DOFade(0, FadeDuration));
        seq.Insert(0, _controlsCanvas.DOFade(0, FadeDuration));
        seq.Append(_pausedCanvas.DOFade(1, FadeDuration));
        _pausedButton.Select();
    }

    public void Exit()
    {
        Application.Quit();
    }

    public static void ToggleOpen()
    {
        _open = !_open;
        if (_open) Hide();
        else MenuStateMachine.ShowMenu("Pause Menu");
    }
}