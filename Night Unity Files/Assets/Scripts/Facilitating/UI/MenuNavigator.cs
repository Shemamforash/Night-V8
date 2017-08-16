using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World;

namespace Facilitating.UI
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

        private static readonly List<MenuButtonPair> menusList = new List<MenuButtonPair>();
        private static readonly Queue<MenuButtonPair> menuHistory = new Queue<MenuButtonPair>();
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
            menusList.Add(new MenuButtonPair(menu, firstButton));
        }

        protected bool IsMenuOpen(string menuName)
        {
            foreach (MenuButtonPair pair in menusList)
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
            foreach (MenuButtonPair menuPair in menusList)
            {
                GameObject menu = menuPair.Menu;
                if (menu.name == target)
                {
                    menu.SetActive(true);
                    if (menu.name == "Game Menu")
                    {
                        menuHistory.Clear();
                    }
                    MenuButtonPair previousMenuButton = null;
                    if (menuHistory.Count > 0)
                    {
                        previousMenuButton = menuHistory.Peek();
                    }
                    if (previousMenuButton != null && target == previousMenuButton.Menu.name)
                    {
                        previousMenuButton.Button.GetComponent<Selectable>().Select();
                        menuHistory.Dequeue();
                    }
                    else
                    {
                        menuPair.Button.GetComponent<Selectable>().Select();
                    }
                    menuHistory.Enqueue(currentMenuButton);
                    break;
                }
                menu.SetActive(false);
            }
        }
    }
}