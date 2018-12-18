using DG.Tweening;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.Libraries;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class PauseMenuController : MonoBehaviour
{
    private bool _open;
    private static bool _fading;
    private static string _lastMenu;
    private static PauseMenuController _instance;
    private static TextMeshProUGUI _title;
    private static CloseButtonController _closeButton;
    private CanvasGroup _background;
    private bool _pauseShown;

    public void Awake()
    {
        _title = gameObject.FindChildWithName<TextMeshProUGUI>("Title");
        _closeButton = gameObject.FindChildWithName<CloseButtonController>("Close Button");
        _instance = this;
        _open = false;
        _background = GetComponent<CanvasGroup>();
        _background.alpha = 0f;
        _closeButton.SetCallback(CloseClicked);
        _closeButton.SetOnClick(CloseClicked);
    }

    private void CloseClicked()
    {
        if (_pauseShown) Hide();
        else ShowPauseMenu();
    }

    private void Show()
    {
        _background.blocksRaycasts = true;
        AudioController.FadeInMusicMuffle();
        _lastMenu = MenuStateMachine.CurrentMenu().gameObject.name;
        _instance.ShowPauseMenu();
        _fading = true;
        Sequence sequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, true);
        sequence.Append(_background.DOFade(1f, 0.25f));
        sequence.AppendCallback(() =>
        {
            _fading = false;
            _open = true;
        });
        Pause();
        _closeButton.Enable();
    }

    public void ReturnToMainMenu()
    {
        SceneChanger.GoToMainMenuScene();
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }

    public void Hide()
    {
        _background.blocksRaycasts = false;
        _closeButton.Disable();
        AudioController.FadeOutMusicMuffle();
        MenuStateMachine.ShowMenu(_lastMenu);
        _fading = true;
        Sequence sequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, true);
        sequence.Append(_background.DOFade(0f, 0.25f));
        sequence.AppendCallback(() =>
        {
            _fading = false;
            _open = false;
            Resume();
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

    private void Resume()
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
                CombatManager.Resume();
                break;
        }
    }

    public void ShowOptions()
    {
        _pauseShown = false;
        _title.text = "Options";
        MenuStateMachine.ShowMenu("Options");
    }

    public void ShowControls()
    {
        _pauseShown = false;
        _title.text = "Controls";
        MenuStateMachine.ShowMenu("Controls");
    }

    public void ShowPauseMenu()
    {
        _pauseShown = true;
        _title.text = "Paused";
        MenuStateMachine.ShowMenu("Pause");
    }

    public void SaveGame()
    {
        SaveIconController.Save();
    }

    public void ExitToDesktop()
    {
        Application.Quit();
    }

    public void ExitToMenu()
    {
        SceneChanger.GoToMainMenuScene();
    }

    public static void ToggleOpen()
    {
        if (_fading) return;
        _instance._open = !_instance._open;
        if (!_instance._open) _instance.Hide();
        else _instance.Show();
    }

    public static bool IsOpen()
    {
        return _instance != null && _instance._open;
    }
}