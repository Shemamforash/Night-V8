using Facilitating.Persistence;
using Game.Characters.CharacterActions;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private bool _starting;

    public void Awake()
    {
        _starting = false;
    }

    public void StartGame(bool newGame)
    {
        _starting = true;
        InputHandler.SetCurrentListener(null);
        if (newGame) StoryController.ShowText(JournalEntry.GetStoryText(1), false);
        else
        {
            SceneChanger.GoToGameScene();
            SceneChanger.FadeInAudio();
        }
    }

    public void ContinueGame()
    {
        if (_starting) return;
        SaveController.LoadGame();
        StartGame(false);
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
        SaveController.SaveGame();
        StartGame(true);
    }

    public void EnableTutorial()
    {
        ClearSaveAndLoad();
        TutorialManager.SetTutorialActive(true);
    }

    public void DisableTutorial()
    {
        ClearSaveAndLoad();
        TutorialManager.SetTutorialActive(false);
    }
}