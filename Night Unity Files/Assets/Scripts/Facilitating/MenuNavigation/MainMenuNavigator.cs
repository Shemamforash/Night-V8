using Facilitating.Persistence;
using Game.Global;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Facilitating.MenuNavigation
{
    public class MainMenuNavigator : MonoBehaviour
    {
        public void Awake()
        {
            Cursor.visible = false;
        }

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
//            SaveController.SaveSettings();
            SaveController.SaveGame();
            LoadGame();
        }

        private void LoadGame()
        {
            WorldState.UnPause();
            SceneManager.LoadScene("Game");
        }

        public void ContinueGame()
        {
            if (SaveController.SaveExists())
            {
//                SaveController.SaveSettings();
                SaveController.LoadGame();
                LoadGame();
            }
            else
            {
                MenuStateMachine.ShowMenu("No Save Warning");
            }
        }

        public void ShowMenu(Menu menu)
        {
            MenuStateMachine.ShowMenu(menu.name);
        }
    }
}