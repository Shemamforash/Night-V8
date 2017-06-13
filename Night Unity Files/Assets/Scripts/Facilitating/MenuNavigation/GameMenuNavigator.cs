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
        public GameObject pauseMainMenu, pauseSubMenu, optionsSubMenu, controlsSubMenu, gameMenu, navigationMenu;
        public Button pauseMenuFirstSelect, controlsFirstSelect, navigationFirstSelect;
        private GameObject gameLastButton;
        public Slider optionsFirstSelect;
        public bool menuButtonDown = false;
        private Environment destinationOption1, destinationOption2;
        public Text destinationOption1Text, destinationOption2Text;

        public GameMenuNavigator()
        {
            WorldState.menuNavigator = this;
        }

        public void Start()
        {
            SaveController.LoadSettings();
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
            WorldState.currentDanger += 1;
        }

        public void MakeDestinationSelection(Text t){
            if(t == destinationOption1Text) {
                WorldState.environmentManager.SetCurrentEnvironment(destinationOption1);
            } else {
                WorldState.environmentManager.SetCurrentEnvironment(destinationOption2);
            }
            navigationMenu.SetActive(false);
            BackToGame();
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
            SaveController.SaveGame();
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