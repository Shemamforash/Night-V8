using Facilitating.Persistence;
using Game.World;
using Game.World.Time;
using Persistence;
using SamsHelper.ReactiveUI;
using SamsHelper.ReactiveUI.MenuSystem;
using TMPro;
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
                MenuStateMachine.Instance().NavigateToState("Overwrite Save Warning");
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
            WorldTime.Instance().UnPause();
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
                MenuStateMachine.Instance().NavigateToState("No Save Warning");
            }
        }

        public void SetDifficulty(GameObject btn)
        {
            string btnDifficulty = btn.transform.Find("Text").GetComponent<TextMeshProUGUI>().text.ToLower();
            GameData.SetDifficultyFromString(btnDifficulty);
        }
    }
}