using Facilitating.Persistence;
using Game.World;
using Game.World.Time;
using Persistence;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
                MenuStateMachine.Instance.NavigateToState("Overwrite Save Warning");
            }
            else
            {
                ClearSaveAndLoad();
            }
        }

        public void ClearSaveAndLoad()
        {
            SaveController.SaveSettings();
            SaveController.SaveGameToFile();
            LoadGame();
        }

        private void LoadGame()
        {
            WorldTime.Instance().UnPause();
            SceneManager.LoadScene("Game");
        }

        public void ContinueGame()
        {
            if (SaveController.SaveExists())
            {
                SaveController.SaveSettings();
                SaveController.LoadGameFromFile();
                LoadGame();
            }
            else
            {
                MenuStateMachine.Instance.NavigateToState("No Save Warning");
            }
        }

        public void SetDifficulty(GameObject btn)
        {
            string btnDifficulty = btn.transform.Find("Text").GetComponent<Text>().text.ToLower();
            GameData.SetDifficultyFromString(btnDifficulty);
        }
    }
}