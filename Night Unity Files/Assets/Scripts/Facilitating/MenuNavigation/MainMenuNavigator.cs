using UnityEngine.UI;
using UnityEngine;

public class MainMenuNavigator : MonoBehaviour
{
    public GameObject newGameSubMenu, mainSubMenu, optionsSubMenu, statsSubMenu, noSaveSubMenu, overwriteSubMenu;
    private enum Difficulty { EASY, NORMAL, HARD };
    private Difficulty selectedDifficulty;
	private bool permadeathOn = true;

    public void BackToMenu()
    {
        newGameSubMenu.SetActive(false);
        optionsSubMenu.SetActive(false);
		noSaveSubMenu.SetActive(false);
		overwriteSubMenu.SetActive(false);
		statsSubMenu.SetActive(false);
        mainSubMenu.SetActive(true);
    }

    public void CloseGame()
    {
        Application.Quit();
    }

	public void OpenStats(){
		mainSubMenu.SetActive(false);
		statsSubMenu.SetActive(true);
	}

	public void StartNewGame(){
		/*if save exists {} */
		newGameSubMenu.SetActive(false);
		overwriteSubMenu.SetActive(true);
		//else{start new game}
	}

	public void ContinueGame(){
		/*if save exists{
			load game
		} else { */
		mainSubMenu.SetActive(false);
		noSaveSubMenu.SetActive(true);
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

	public void TogglePermadeath(GameObject button){
		Text buttonText = button.transform.Find("Text").GetComponent<Text>();
		if(buttonText.text.ToLower() == "on"){
			buttonText.text = "OFF";
			permadeathOn = false;
		} else {
			buttonText.text = "ON";
			permadeathOn = true;
		}
	}
}
