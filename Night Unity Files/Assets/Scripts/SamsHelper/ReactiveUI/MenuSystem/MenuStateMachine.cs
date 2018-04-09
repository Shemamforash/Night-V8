using Facilitating.Persistence;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;
using UnityEngine;

namespace SamsHelper.ReactiveUI.MenuSystem
{
    public class MenuStateMachine : MonoBehaviour
    {
        private static MenuStateMachine _instance;
        public static readonly StateMachine States = new StateMachine();
        public Menu InitialMenu;

        public void Awake()
        {
            _instance = this;
            foreach (Menu t in Helper.FindAllComponentsInChildren<Menu>(transform))
            {
                MenuState menu = new MenuState(States, t.name, t);
                if (!t.gameObject.activeInHierarchy)
                {
                    t.gameObject.SetActive(true);
                    t.gameObject.SetActive(false);
                }

                States.AddState(menu);
            }

            if (InitialMenu != null) ShowMenu(InitialMenu.name);
        }

        public static void ShowMenu(string menuName)
        {
            States.GetState(menuName).Enter();
        }

        //TODO move me somewhere more suitable
        public void OnApplicationQuit()
        {
            SaveController.SaveSettings();
            SaveController.SaveGame();
        }

        public static void HideCurrentMenu()
        {
            ((MenuState)States.GetCurrentState()).SetActive(false);
        }

        public static void ShowCurrentMenu()
        {
            ((MenuState)States.GetCurrentState()).SetActive(true);
        }

        public static void GoToInitialMenu()
        {
            ShowMenu(_instance.InitialMenu.name);
        }
    }
}