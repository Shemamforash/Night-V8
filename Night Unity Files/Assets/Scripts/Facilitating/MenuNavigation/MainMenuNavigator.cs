using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuNavigator : MonoBehaviour
{
    public GameObject newGameSubMenu, mainSubMenu, optionsSubMenu, statsSubMenu, noSaveSubMenu, overwriteSubMenu, controlsSubMenu;
	public Button newGameFirstSelect, mainMenuFirstSelect, noSaveFirstSelect, overwriteFirstSelect, controlsFirstSelect;
	public Slider optionsFirstSelect;
	public Scrollbar statsFirstSelect;

    private enum Difficulty { EASY, NORMAL, HARD };
    private Difficulty selectedDifficulty;

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

    public void OpenControls(){
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

	public void ClearSaveAndLoad(){
		//TODO create new game
        SaveController.Save();
        // SceneManager.LoadScene("Game");
	}

    public void ContinueGame()
    {
        if (SaveController.SaveExists())
        {
            //TODO load old game
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
        switch (btnDifficulty)
        {
            case "easy":
                selectedDifficulty = Difficulty.EASY;
                break;
            case "normal":
                selectedDifficulty = Difficulty.NORMAL;
                break;
            case "hard":
                selectedDifficulty = Difficulty.HARD;
                break;
            default:
                print("No Difficulty Selected");
                break;
        }
    }
}
