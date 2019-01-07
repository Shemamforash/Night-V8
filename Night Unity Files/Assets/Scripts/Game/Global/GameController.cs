using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private static bool _starting;

    public void Awake()
    {
        _starting = false;
    }

    private void OnDestroy()
    {
        _starting = false;
    }

    public static void StartGame(bool newGame)
    {
        _starting = true;
        InputHandler.SetCurrentListener(null);
        if (newGame) StoryController.ShowText(JournalEntry.GetStoryText());
        else
        {
            SceneChanger.GoToGameScene();
            SceneChanger.FadeInAudio();
        }
    }

    public void ContinueGame()
    {
        if (_starting) return;
        MenuStateMachine.ShowMenu("Load Save Menu");
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }

    public void GoToMenu()
    {
        SceneChanger.GoToMainMenuScene();
    }

    private void ClearSaveAndLoad()
    {
        if (_starting) return;
        _starting = true;
        SaveController.ClearSave();
        WorldState.ResetWorld();
        SaveController.ManualSave();
        StartGame(true);
    }

    public void EnableTutorial()
    {
        TutorialManager.SetTutorialActive(true);
        ClearSaveAndLoad();
    }

    public void DisableTutorial()
    {
        ClearSaveAndLoad();
        TutorialManager.SetTutorialActive(false);
    }
}