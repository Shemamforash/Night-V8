using UnityEngine.UI;
using UnityEngine;

public class MainMenuNavigator : MonoBehaviour
{
    public GameObject newGameSubMenu, mainSubMenu, optionsSubMenu;
    private enum Difficulty { EASY, NORMAL, HARD };
    private Difficulty selectedDifficulty;

    public void BackToMenu()
    {
        newGameSubMenu.SetActive(false);
        optionsSubMenu.SetActive(false);
        mainSubMenu.SetActive(true);
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void OpenNewGameMenu()
    {
        mainSubMenu.SetActive(false);
        newGameSubMenu.SetActive(true);
    }

	public void OpenOptionsMenu(){
		mainSubMenu.SetActive(false);
		optionsSubMenu.SetActive(true);
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
