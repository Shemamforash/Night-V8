using System;
using Characters;
using Game.Misc;
using UnityEditor;

namespace Menus
{
    using Persistence;
    using Audio;
    using World;
    using UnityEngine.UI;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.EventSystems;

    public class GameMenuNavigator : MonoBehaviour
    {
        public GameObject pauseMainMenu, pauseSubMenu, optionsSubMenu, controlsSubMenu, gameMenu, navigationMenu, gameOverMenu, actionDurationMenu;
        public Button pauseMenuFirstSelect, controlsFirstSelect, navigationFirstSelect;
        private GameObject gameLastButton;
        public Slider optionsFirstSelect;
        public bool menuButtonDown = false;
        private Environment destinationOption1, destinationOption2;
        public Text destinationOption1Text, destinationOption2Text, actionDurationText;
        private Character.CharacterAction currentAction;
        private int _actionDuration;
        
        public GameMenuNavigator()
        {
            WorldState.MenuNavigator = this;
        }

        public void Start()
        {
            SaveController.LoadSettings();
            SaveController.LoadGameFromFile();
            GetComponent<GlobalAudioManager>().Initialise();
        }

        public void ShowDestinationChoices(Environment option1, Environment option2)
        {
            gameLastButton = EventSystem.current.currentSelectedGameObject;
            gameMenu.SetActive(false);
            navigationMenu.SetActive(true);
            navigationFirstSelect.Select();
            destinationOption1 = option1;
            destinationOption2 = option2;
            destinationOption1Text.text = option1.EnvironmentName;
            destinationOption2Text.text = option2.EnvironmentName;
            WorldTime.Pause();
            WorldState.CurrentDanger += 1;
        }

        public void ShowActionDurationMenu(Character.CharacterAction a)
        {
            WorldTime.Pause();
            actionDurationMenu.SetActive(true);
            _actionDuration = a.Duration;
            actionDurationText.text = _actionDuration + "hrs"; 
            Helper.FindChildWithName(actionDurationMenu, "Confirm").GetComponent<Button>().Select();
            gameMenu.SetActive(false);
            currentAction = a;
        }

        public void CloseActionDurationMenu()
        {
            WorldTime.UnPause();
            actionDurationMenu.SetActive(false);
            gameMenu.SetActive(true);
            currentAction.Duration = _actionDuration;
            currentAction.InitialiseAction();
            currentAction.Parent().CharacterUi.CollapseCharacterButton.Select();
        }

        public void IncrementActionDuration()
        {
            ++_actionDuration;
            actionDurationText.text = _actionDuration + "hrs";
        }

        public void DecrementActionDuration()
        {
            --_actionDuration;
            actionDurationText.text = _actionDuration + "hrs";
        }
        
        public void MakeDestinationSelection(Text t){
            if(t == destinationOption1Text) {
                WorldState.EnvironmentManager.SetCurrentEnvironment(destinationOption1);
            } else {
                WorldState.EnvironmentManager.SetCurrentEnvironment(destinationOption2);
            }
            navigationMenu.SetActive(false);
            BackToGame();
        }

        public void EndGameFail()
        {
            WorldTime.Pause();
            gameOverMenu.SetActive(true);
            gameMenu.SetActive(false);
        }

        public void Update()
        {
            if (Input.GetAxis("Menu") != 0)
            {
                if (!menuButtonDown)
                {
                    menuButtonDown = true;
                    TogglePauseMenu();
                }
            }
            else
            {
                menuButtonDown = false;
            }
        }

        public void TogglePauseMenu()
        {
            if (pauseSubMenu.activeInHierarchy)
            {
                pauseMainMenu.SetActive(false);
                BackToGame();
            }
            else
            {
                gameMenu.SetActive(false);
                gameLastButton = EventSystem.current.currentSelectedGameObject;
                WorldTime.Pause();
                BackToMenu();
            }
        }

        private void BackToGame(){
            gameMenu.SetActive(true);
            gameLastButton.GetComponent<Selectable>().Select();
            WorldTime.UnPause();
        }

        public void OnApplicationQuit()
        {
            Save();
        }

        public void BackToMenu()
        {
            pauseMainMenu.SetActive(true);
            optionsSubMenu.SetActive(false);
            controlsSubMenu.SetActive(false);
            pauseSubMenu.SetActive(true);
            pauseMenuFirstSelect.Select();
        }

        private void Save()
        {
            SaveController.SaveSettings();
            SaveController.SaveGameToFile();
        }

        public void ExitAndSave()
        {
            Save();
            SceneManager.LoadScene("Menu");
        }

        public void OpenControls()
        {
            pauseSubMenu.SetActive(false);
            controlsSubMenu.SetActive(true);
            controlsFirstSelect.Select();
        }

        public void OpenOptionsMenu()
        {
            pauseSubMenu.SetActive(false);
            optionsSubMenu.SetActive(true);
            optionsFirstSelect.Select();
        }
    }
}