using Facilitating.Persistence;
using Persistence;
using SamsHelper.BaseGameFunctionality.StateMachines;

namespace SamsHelper.ReactiveUI.MenuSystem
{
    public class MenuStateMachine : StateMachine
    {
        private static MenuStateMachine _instance;
        public Menu InitialMenu;
        
        public void Awake()
        {
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

        public static MenuStateMachine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<MenuStateMachine>();
                }
                return _instance;
            }
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