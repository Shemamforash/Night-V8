using Facilitating.Persistence;
using Game.Global;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Facilitating.MenuNavigation
{
    public class MainMenuNavigator : MonoBehaviour
    {
        public void CloseGame()
        {
            Application.Quit();
        }

        public void StartNewGame()
        {
            if (SaveController.SaveExists())
                MenuStateMachine.ShowMenu("Overwrite Save Warning");
            else
                ClearSaveAndLoad();
        }

        public void ClearSaveAndLoad()
        {
            SaveController.SaveSettings();
            SaveController.SaveGame();
            LoadGame();
        }

        private void LoadGame()
        {
            WorldState.UnPause();
            SceneManager.LoadScene("Game");
        }

        public void GoToNewGameMenu()
        {
            MenuStateMachine.ShowMenu("Difficulty Menu");
        }

        public void ContinueGame()
        {
            if (SaveController.SaveExists())
            {
                SaveController.SaveSettings();
                SaveController.LoadGame();
                LoadGame();
            }
            else
            {
                MenuStateMachine.ShowMenu("No Save Warning");
            }
        }

        public void SetDifficulty(GameObject btn)
        {
            string btnDifficulty = btn.transform.Find("Text").GetComponent<TextMeshProUGUI>().text.ToLower();
            GameData.SetDifficultyFromString(btnDifficulty);
        }
    }
}