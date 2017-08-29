using System;
using Audio;
using Characters;
using Facilitating.UI;
using Game.Characters;
using Game.Characters.CharacterActions;
using Game.World;
using Persistence;
using SamsHelper;
using SamsHelper.ReactiveUI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using World;

namespace Facilitating.MenuNavigation
{
    public class GameMenuNavigator : MenuNavigator
    {
        private bool _menuButtonDown;
        public Text ActionDurationText;
        private BaseCharacterAction _currentAction;
        private int _actionDuration;
        private bool _pauseMenuOpen;
        public static GameMenuNavigator MenuNavigator;

        public void Awake()
        {
            MenuNavigator = this;
            
            SaveController.LoadSettings();
            SaveController.LoadGameFromFile();
            Camera.main.GetComponent<GlobalAudioManager>().Initialise();

            WorldState.MenuNavigator = this;
            ActionDurationText = Helper.FindChildWithName<Transform>(gameObject, "Duration").GetComponent<Text>();

            AddMenu("Pause Sub Menu", "Resume");
            AddMenu("Options Container", "Master Slider");
            AddMenu("Controls Container", "Back");
            AddMenu("Game Menu", "Food");
            AddMenu("DestinationMenu", "Option 1");
            AddMenu("Game Over Menu", "Menu");
            AddMenu("Action Duration Menu", "Confirm");
            AddMenu("Combat Menu", "Approach");
            AddMenu("Region Menu", "Back");

            SetInitialMenu(GameObject.Find("Game Menu"), GameObject.Find("Food"));

            Debug.Log(EventSystem.current.currentSelectedGameObject);
        }

        public override void SwitchToMenu(string menu, bool paused)
        {
            CharacterManager.ExitCharacter();
            base.SwitchToMenu(menu, paused);
        }

        public void ShowActionDurationMenu(BaseCharacterAction a)
        {
            SwitchToMenu("Action Duration Menu", true);
            _currentAction = a;
            ActionDurationText.text = _currentAction.GetCostAsString();
        }

        public void CloseActionDurationMenu()
        {
            SwitchToMenu("Game Menu", false);
            _currentAction.Start();
        }

        public void IncrementActionDuration()
        {
            _currentAction.IncreaseDuration();
            ActionDurationText.text = _currentAction.GetCostAsString();
        }

        public void DecrementActionDuration()
        {
            _currentAction.DecreaseDuration();
            ActionDurationText.text = _currentAction.GetCostAsString();
        }

        public void EndGameFail()
        {
            SwitchToMenu("Game Over Menu", true);
        }

        public void Update()
        {
            if (Input.GetAxis("Menu") != 0)
            {
                if (!_menuButtonDown)
                {
                    _menuButtonDown = true;
                    TogglePauseMenu();
                }
            }
            else
            {
                _menuButtonDown = false;
            }
        }

        public void TogglePauseMenu()
        {
            if (!_pauseMenuOpen)
            {
                SwitchToMenu("Pause Sub Menu", true);
            }
            else
            {
                SwitchToMenu("Game Menu", false);
            }
            _pauseMenuOpen = !_pauseMenuOpen;
        }

        public void BackToPauseMenu()
        {
            SwitchToMenu("Pause Sub Menu", true);
        }

        public void OnApplicationQuit()
        {
            Save();
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
            SwitchToMenu("Controls Container", true);
        }

        public void OpenOptionsMenu()
        {
            SwitchToMenu("Options Container", true);
        }
    }
}