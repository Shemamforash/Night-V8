using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameMenuNavigator : MonoBehaviour
{
    public GameObject pauseMainMenu, pauseSubMenu, optionsSubMenu, controlsSubMenu, gameMenu;
    public Button pauseMenuFirstSelect, controlsFirstSelect;
    private GameObject gameLastButton;
    public Slider optionsFirstSelect;
    public bool menuButtonDown = false;

    public void Start()
    {
        SaveController.LoadSettings();
        GetComponent<GlobalAudioManager>().Initialise();
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
            gameMenu.SetActive(true);
            gameLastButton.GetComponent<Selectable>().Select();
            WorldTime.UnPause();
        }
        else
        {
            gameMenu.SetActive(false);
            gameLastButton = EventSystem.current.currentSelectedGameObject;
            WorldTime.Pause();
            BackToMenu();
        }
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
