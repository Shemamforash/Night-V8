using System.Collections;
using DG.Tweening;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class PauseMenuController : MonoBehaviour
{
    private static bool _open;
    private static bool _fading;
    private static string _lastMenu;
    private static PauseMenuController _instance;
    private CanvasGroup _background;

    public void Awake()
    {
        _instance = this;
        _open = false;
        _background = GetComponent<CanvasGroup>();
        _background.alpha = 0f;
    }

    private void Show()
    {
        VolumeController.FadeInMuffle();
        _lastMenu = MenuStateMachine.CurrentMenu().gameObject.name;
        _instance.ShowPauseMenu();
        _fading = true;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_background.DOFade(1f, 0.25f));
        sequence.AppendCallback(() =>
        {
            _fading = false;
            _open = true;
        });
        Pause();
    }

    public void ReturnToMainMenu()
    {
        SaveController.QuickSave();
        SceneChanger.GoToMainMenuScene();
    }

    public void QuitToDesktop()
    {
        SaveController.QuickSave();
        Application.Quit();
    }

    public void Hide()
    {
        VolumeController.FadeOutMuffle();
        MenuStateMachine.ShowMenu(_lastMenu);
        _fading = true;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_background.DOFade(0f, 0.25f));
        sequence.AppendCallback(() =>
        {
            _fading = false;
            _open = false;
            Unpause();
        });
    }

    private void Pause()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Story":
                StoryController.Pause();
                break;
            case "Credits":
                CreditsController.Pause();
                break;
            case "Combat":
                CombatManager.Pause();
                break;
        }
    }

    private void Unpause()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Story":
                StoryController.Unpause();
                break;
            case "Credits":
                CreditsController.Unpause();
                break;
            case "Combat":
                CombatManager.Unpause();
                break;
        }
    }

    public void ShowOptions()
    {
        MenuStateMachine.ShowMenu("Options");
    }

    public void ShowControls()
    {
        MenuStateMachine.ShowMenu("Controls");
    }

    public void ShowPauseMenu()
    {
        MenuStateMachine.ShowMenu("Pause");
    }

    public void ExitToDesktop()
    {
        SaveController.QuickSave();
        Application.Quit();
    }

    public void ExitToMenu()
    {
        SaveController.QuickSave();
        SceneChanger.GoToMainMenuScene();
    }

    public static void ToggleOpen()
    {
        if (_fading) return;
        _open = !_open;
        if (!_open) _instance.Hide();
        else _instance.Show();
    }
}