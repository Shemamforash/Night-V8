using Facilitating.Persistence;
using Game.World;
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
            {
                MenuStateMachine.States.NavigateToState("Overwrite Save Warning");
            }
            else
            {
                ClearSaveAndLoad();
            }
        }

        public void ClearSaveAndLoad()
        {
            SaveController.SaveSettings();
            SaveController.SaveGame();
            LoadGame();
        }

        private void LoadGame()
        {
            WorldState.Instance().UnPause();
            SceneManager.LoadScene("Game");
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
                MenuStateMachine.States.NavigateToState("No Save Warning");
            }
        }

        public void SetDifficulty(GameObject btn)
        {
            string btnDifficulty = btn.transform.Find("Text").GetComponent<TextMeshProUGUI>().text.ToLower();
            GameData.SetDifficultyFromString(btnDifficulty);
        }
    }
}