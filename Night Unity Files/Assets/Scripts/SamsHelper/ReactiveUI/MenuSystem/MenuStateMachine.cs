using Facilitating.Persistence;
using SamsHelper.BaseGameFunctionality.StateMachines;

namespace SamsHelper.ReactiveUI.MenuSystem
{
    public class MenuStateMachine : StateMachine
    {
        public Menu InitialMenu;
        private static MenuStateMachine _instance;

        public static MenuStateMachine Instance()
        {
            return _instance ?? FindObjectOfType<MenuStateMachine>();
        }
        
        public void Awake()
        {
            _instance = this;
            foreach (Menu t in Helper.FindAllComponentsInChildren<Menu>(transform))
            {
                MenuState menu = new MenuState(t.name, t, this);
                if (!t.gameObject.activeInHierarchy)
                {
                    t.gameObject.SetActive(true);
                    t.gameObject.SetActive(false);
                }
                AddState(menu);
            }
            NavigateToState(InitialMenu.name);
        }

        //TODO move me somewhere more suitable
        public void OnApplicationQuit()
        {
            SaveController.SaveSettings();
            SaveController.SaveGame();
        }

        public void GoToMenu(Menu m)
        {
            NavigateToState(m.gameObject.name);
        }

        public void GoToInitialMenu()
        {
            NavigateToState(InitialMenu.name);
        }
    }
}