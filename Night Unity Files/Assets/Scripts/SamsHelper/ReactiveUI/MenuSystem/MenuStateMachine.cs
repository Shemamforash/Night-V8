using Facilitating.Persistence;
using SamsHelper.BaseGameFunctionality.StateMachines;
using UnityEngine;

namespace SamsHelper.ReactiveUI.MenuSystem
{
    public class MenuStateMachine : MonoBehaviour
    {
        public Menu InitialMenu;
        private static MenuStateMachine _instance;
        public static readonly StateMachine<MenuState> States = new StateMachine<MenuState>();

        public void Awake()
        {
            _instance = this;
            foreach (Menu t in Helper.FindAllComponentsInChildren<Menu>(transform))
            {
                MenuState menu = new MenuState(t.name, t);
                if (!t.gameObject.activeInHierarchy)
                {
                    t.gameObject.SetActive(true);
                    t.gameObject.SetActive(false);
                }
                States.AddState(menu);
            }
            if (InitialMenu != null)
            {
                States.NavigateToState(InitialMenu.name);
            }
        }

        //TODO move me somewhere more suitable
        public void OnApplicationQuit()
        {
            SaveController.SaveSettings();
            SaveController.SaveGame();
        }

        public static void HideCurrentMenu()
        {
            States.GetCurrentState().GetGameObject().SetActive(false);
        }

        public static void ShowCurrentMenu()
        {
            States.GetCurrentState().GetGameObject().SetActive(true);
        }

        public static void GoToInitialMenu()
        {
            States.NavigateToState(_instance.InitialMenu.name);
        }
    }
}