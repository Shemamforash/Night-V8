using DG.Tweening;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Generation;
using Game.Exploration.Environment;
using Game.Exploration.Regions;
using Game.Global;
using SamsHelper.Input;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        TryFadeMusic();
        if (TryContinueFromTutorial()) return;
        if (TryContinueFromStory(newGame)) return;
        ContinueFromGame();
    }

    private static void TryFadeMusic()
    {
        GameObject musicAudio = GameObject.Find("Music Audio");
        if (musicAudio == null) return;
        musicAudio.GetComponent<AudioSource>().DOFade(0f, 0.5f).SetUpdate(true);
    }


    private static void ContinueFromGame()
    {
        SceneChanger.GoToGameScene();
        SceneChanger.FadeInAudio();
    }

    private static bool TryContinueFromStory(bool newGame)
    {
        if (!newGame && StoryController.StorySeen) return false;
        StoryController.Show();
        return true;
    }

    private static bool TryContinueFromTutorial()
    {
        if (!TutorialManager.Active() || TutorialManager.FinishedIntroTutorial()) return false;
        Region region = new Region();
        region.SetRegionType(RegionType.Tutorial);
        CharacterManager.SelectedCharacter.TravelAction.SetCurrentRegion(region);
        SceneChanger.GoToCombatScene(CharacterManager.SelectedCharacter);
        return true;
    }

    public void ContinueGame()
    {
        if (_starting) return;
        if (SceneManager.GetActiveScene().name == "Menu") MenuStateMachine.ShowMenu("Load Save Menu");
        else LoadSaveMenu.LoadMostRecentSave();
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
        TutorialManager.SetTutorialActive(false);
        ClearSaveAndLoad();
    }

    public void SetDifficultyEasy()
    {
        WorldState.SetDifficultyEasy();
        MenuStateMachine.ShowMenu("Tutorial Choice");
    }

    public void SetDifficultyHard()
    {
        WorldState.SetDifficultyHard();
        MenuStateMachine.ShowMenu("Tutorial Choice");
    }
}