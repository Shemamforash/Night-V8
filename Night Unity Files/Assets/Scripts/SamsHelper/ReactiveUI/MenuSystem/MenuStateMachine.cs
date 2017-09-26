using Facilitating.Persistence;
using SamsHelper.BaseGameFunctionality.StateMachines;
using UnityEngine;

namespace SamsHelper.ReactiveUI.MenuSystem
{
    public class MenuStateMachine : MonoBehaviour
    {
        public Menu InitialMenu;
        private static MenuStateMachine _instance;
        public static readonly StateMachine States = new StateMachine();

        public void Awake()
        {
            _instance = this;
            foreach (Menu t in Helper.FindAllComponentsInChildren<Menu>(transform))
            {
                MenuState menu = new MenuState(t.name, t, States);
                if (!t.gameObject.activeInHierarchy)
                {
                    t.gameObject.SetActive(true);
                    t.gameObject.SetActive(false);
                }
                States.AddState(menu);
            }
            States.NavigateToState(InitialMenu.name);
        }

        //TODO move me somewhere more suitable
        public void OnApplicationQuit()
        {
            SaveController.SaveSettings();
            SaveController.SaveGame();
        }

        public static void GoToMenu(Menu m)
        {
            States.NavigateToState(m.gameObject.name);
        }

        public static void HideCurrentMenu()
        {
            States.GetCurrentState().GameObject.SetActive(false);
        }

        public static void ShowCurrentMenu()
        {
            States.GetCurrentState().GameObject.SetActive(true);
        }

        public static void GoToInitialMenu()
        {
            States.NavigateToState(_instance.InitialMenu.name);
        }
    }
}