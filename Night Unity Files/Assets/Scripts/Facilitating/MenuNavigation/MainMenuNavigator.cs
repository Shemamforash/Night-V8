using Facilitating.Persistence;
using Game.Global;
using SamsHelper.ReactiveUI.MenuSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Menu = SamsHelper.ReactiveUI.MenuSystem.Menu;

namespace Facilitating.MenuNavigation
{
    public class MainMenuNavigator : MonoBehaviour
    {
        public void Awake()
        {
            Cursor.visible = false;
//            ClearSaveAndLoad();
//            EditorApplication.isPlaying = false;
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

        private void ClearSaveAndLoad()
        {
            SaveController.ClearSave();
            WorldState.ResetWorld();
            SaveController.SaveGame();
            StartGame();
        }

        private void StartGame()
        {
            WorldState.UnPause();
            SceneChanger.ChangeScene("Game");
        }

        public void ContinueGame()
        {
            if (SaveController.SaveExists())
            {
//                SaveController.SaveSettings();
                SaveController.LoadGame();
                StartGame();
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