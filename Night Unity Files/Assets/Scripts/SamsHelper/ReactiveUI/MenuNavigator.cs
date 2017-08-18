using System.Collections.Generic;
using Game.World;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI
{
    public abstract class MenuNavigator : MonoBehaviour
    {
        private class MenuButtonPair
        {
            public readonly GameObject Menu, Button;

            public MenuButtonPair(GameObject menu, GameObject button)
            {
                Menu = menu;
                Button = button;
            }
        }

        private static readonly List<MenuButtonPair> MenusList = new List<MenuButtonPair>();
        private static readonly Queue<MenuButtonPair> MenuHistory = new Queue<MenuButtonPair>();
        private static GameObject _currentMenu;

        protected void SetInitialMenu(GameObject startMenu, GameObject startHighlight)
        {
            _currentMenu = startMenu;
            startHighlight.GetComponent<Selectable>().Select();
        }

        protected void AddMenu(string menuName, string firstButtonName)
        {
            GameObject menu = Helper.FindChildWithName(transform, menuName).gameObject;
            GameObject firstButton = Helper.FindChildWithName(menu.transform, firstButtonName).gameObject;
            MenusList.Add(new MenuButtonPair(menu, firstButton));
        }

        protected bool IsMenuOpen(string menuName)
        {
            foreach (MenuButtonPair pair in MenusList)
            {
                if (menuName == pair.Menu.name && pair.Menu.activeInHierarchy)
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void SwitchToMenu(string target, bool pause)
        {
            if (pause)
            {
                WorldTime.Pause();
            }
            else
            {
                WorldTime.UnPause();
            }
            MenuButtonPair currentMenuButton =
                new MenuButtonPair(_currentMenu, EventSystem.current.currentSelectedGameObject);
            foreach (MenuButtonPair menuPair in MenusList)
            {
                GameObject menu = menuPair.Menu;
                if (menu.name == target)
                {
                    menu.SetActive(true);
                    if (menu.name == "Game Menu")
                    {
                        MenuHistory.Clear();
                    }
                    MenuButtonPair previousMenuButton = null;
                    if (MenuHistory.Count > 0)
                    {
                        previousMenuButton = MenuHistory.Peek();
                    }
                    if (previousMenuButton != null && target == previousMenuButton.Menu.name)
                    {
                        previousMenuButton.Button.GetComponent<Selectable>().Select();
                        MenuHistory.Dequeue();
                    }
                    else
                    {
                        menuPair.Button.GetComponent<Selectable>().Select();
                    }
                    MenuHistory.Enqueue(currentMenuButton);
                    break;
                }
                menu.SetActive(false);
            }
        }
    }
}