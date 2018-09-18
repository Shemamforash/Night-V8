using System.Collections;
using DG.Tweening;
using Facilitating.Persistence;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.ReactiveUI.Elements;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class PauseMenuController : MonoBehaviour
{
    private static bool _open;
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
        _lastMenu = MenuStateMachine.CurrentMenu().gameObject.name;
        _instance.ShowPauseMenu();
        _open = true;
        StartCoroutine(FadeInBackground());
        Pause();
    }

    private IEnumerator FadeInBackground()
    {
        float time = 0f;
        while (time < 0.5f)
        {
            time += Time.unscaledDeltaTime;
            if (time > 0.5f) time = 0.5f;
            _background.alpha = time / 0.5f;
            yield return null;
        }
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
        MenuStateMachine.ShowMenu(_lastMenu);
        _open = false;
        Unpause();
        _background.DOFade(0, 0.5f);
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
        Application.Quit();
    }

    public void ExitToMenu()
    {
        SceneChanger.GoToMainMenuScene();
    }

    public static void ToggleOpen()
    {
        _open = !_open;
        if (!_open) _instance.Hide();
        else _instance.Show();
    }
}