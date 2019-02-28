﻿using DG.Tweening;
using Facilitating.Persistence;
using Game.Characters;
using Game.Combat.Generation;
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
        if (TutorialManager.Active() && !TutorialManager.FinishedIntroTutorial())
        {
            Region region = new Region();
            region.SetRegionType(RegionType.Tutorial);
            CharacterManager.SelectedCharacter.TravelAction.SetCurrentRegion(region);
            SceneChanger.GoToCombatScene();
            return;
        }

        if (newGame || !StoryController.StorySeen) StoryController.Show();
        else
        {
            SceneChanger.GoToGameScene();
            SceneChanger.FadeInAudio();
        }

        GameObject.Find("Music Audio").GetComponent<AudioSource>().DOFade(0f, 0.5f).SetUpdate(true);
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