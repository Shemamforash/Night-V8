using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuNavigator : MonoBehaviour
{
    public GameObject newGameSubMenu, mainSubMenu, optionsSubMenu, statsSubMenu, noSaveSubMenu, overwriteSubMenu, controlsSubMenu;
    public Button newGameFirstSelect, mainMenuFirstSelect, noSaveFirstSelect, overwriteFirstSelect, controlsFirstSelect;
    public Slider optionsFirstSelect;
    public Scrollbar statsFirstSelect;

    public void Start()
    {
        SaveController.LoadSettings();
        GetComponent<GlobalAudioManager>().Initialise();
    }

    //TODO move me somewhere more suitable
    public void OnApplicationQuit()
    {
        SaveController.SaveSettings();
        SaveController.SaveGame();
    }

    public void BackToMenu()
    {
        newGameSubMenu.SetActive(false);
        optionsSubMenu.SetActive(false);
        noSaveSubMenu.SetActive(false);
        overwriteSubMenu.SetActive(false);
        statsSubMenu.SetActive(false);
        controlsSubMenu.SetActive(false);
        mainSubMenu.SetActive(true);
        mainMenuFirstSelect.Select();
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void OpenControls()
    {
        mainSubMenu.SetActive(false);
        controlsSubMenu.SetActive(true);
        controlsFirstSelect.Select();
    }

    public void OpenStats()
    {
        mainSubMenu.SetActive(false);
        statsSubMenu.SetActive(true);
        statsFirstSelect.Select();
    }

    public void StartNewGame()
    {
        if (SaveController.SaveExists())
        {
            newGameSubMenu.SetActive(false);
            overwriteSubMenu.SetActive(true);
            overwriteFirstSelect.Select();
        }
        else
        {
            ClearSaveAndLoad();
        }
    }

    public void ClearSaveAndLoad()
    {
        //TODO create new game
        SaveController.SaveGame();
        // SceneManager.LoadScene("Game");
    }

    public void ContinueGame()
    {
        if (SaveController.SaveExists())
        {
            SaveController.LoadGame();
            // SceneManager.LoadScene("Game");
        }
        else
        {
            mainSubMenu.SetActive(false);
            noSaveSubMenu.SetActive(true);
            noSaveFirstSelect.Select();
        }
    }

    public void OpenNewGameMenu()
    {
        mainSubMenu.SetActive(false);
        newGameSubMenu.SetActive(true);
        newGameFirstSelect.Select();
    }

    public void OpenOptionsMenu()
    {
        mainSubMenu.SetActive(false);
        optionsSubMenu.SetActive(true);
        optionsFirstSelect.Select();
    }

    public void SetDifficulty(GameObject btn)
    {
        string btnDifficulty = btn.transform.Find("Text").GetComponent<Text>().text.ToLower();
        Settings.SetDifficultyFromString(btnDifficulty);
    }
}
