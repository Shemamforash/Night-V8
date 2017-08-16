using Audio;
using Characters;
using Facilitating.UI;
using Game.Characters;
using Persistence;
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
        private Environment _destinationOption1, _destinationOption2;
        public Text DestinationOption1Text, DestinationOption2Text, ActionDurationText;
        private CharacterAction _currentAction;
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
            DestinationOption1Text = Helper.FindChildWithName<Transform>(gameObject, "Option 1").Find("Text")
                .GetComponent<Text>();
            DestinationOption2Text = Helper.FindChildWithName<Transform>(gameObject, "Option 2").Find("Text")
                .GetComponent<Text>();
            ActionDurationText = Helper.FindChildWithName<Transform>(gameObject, "Duration").GetComponent<Text>();

            AddMenu("Pause Sub Menu", "Resume");
            AddMenu("Options Container", "Master Slider");
            AddMenu("Controls Container", "Back");
            AddMenu("Game Menu", "Food");
            AddMenu("DestinationMenu", "Option 1");
            AddMenu("Game Over Menu", "Menu");
            AddMenu("Action Duration Menu", "Confirm");
            AddMenu("Combat Menu", "Approach");

            SetInitialMenu(GameObject.Find("Game Menu"), GameObject.Find("Food"));

            Debug.Log(EventSystem.current.currentSelectedGameObject);
        }

        public override void SwitchToMenu(string menu, bool paused)
        {
            CharacterManager.ExitCharacter();
            base.SwitchToMenu(menu, paused);
        }

        public void ShowDestinationChoices(Environment option1, Environment option2)
        {
            SwitchToMenu("DestinationMenu", true);
            _destinationOption1 = option1;
            _destinationOption2 = option2;
            DestinationOption1Text.text = option1.EnvironmentName;
            DestinationOption2Text.text = option2.EnvironmentName;
            WorldState.CurrentDanger += 1;
        }

        public void ShowActionDurationMenu(CharacterAction a)
        {
            SwitchToMenu("Action Duration Menu", true);
            _actionDuration = a.Duration;
            ActionDurationText.text = _actionDuration + "hrs";
            _currentAction = a;
        }

        public void CloseActionDurationMenu()
        {
            SwitchToMenu("Game Menu", false);
            _currentAction.Duration = _actionDuration;
            _currentAction.InitialiseAction();
        }

        public void IncrementActionDuration()
        {
            ++_actionDuration;
            ActionDurationText.text = _actionDuration + "hrs";
        }

        public void DecrementActionDuration()
        {
            if (_actionDuration > 1)
            {
                --_actionDuration;
                ActionDurationText.text = _actionDuration + "hrs";
            }
        }

        public void MakeDestinationSelection(Text t)
        {
            SwitchToMenu("Game Menu", false);
            if (t == DestinationOption1Text)
            {
                WorldState.EnvironmentManager.SetCurrentEnvironment(_destinationOption1);
            }
            else
            {
                WorldState.EnvironmentManager.SetCurrentEnvironment(_destinationOption2);
            }
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