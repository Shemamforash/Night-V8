﻿using System.Collections;
using Facilitating.Persistence;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SamsHelper.ReactiveUI.MenuSystem
{
    public class MenuStateMachine : MonoBehaviour
    {
        private static MenuStateMachine _instance;
        public static StateMachine States;
        public Menu InitialMenu;

        public void Awake()
        {
            States = new StateMachine();
            _instance = this;
            foreach (Menu t in Helper.FindAllComponentsInChildren<Menu>(transform))
            {
                MenuState menu = new MenuState(States, t.name, t);
                if (!t.gameObject.activeInHierarchy)
                {
                    t.gameObject.SetActive(true);
                    t.GetComponent<CanvasGroup>().alpha = 0;
                }

                States.AddState(menu);
            }
        }

        public void Start()
        {
            if (InitialMenu != null) ShowMenu(InitialMenu.name);
        }

        public static void ShowMenu(string menuName)
        {
            if (States.GetCurrentState()?.Name == menuName) return;
            _instance.StartCoroutine(_instance.FadeMenu(menuName));
        }

        private IEnumerator FadeMenu(string menuName)
        {
            float fadeTime = 0.1f;
            float currentTime = fadeTime;
            EventSystem.current.sendNavigationEvents = false;
            
            MenuState currentState = (MenuState) States.GetCurrentState();
            if (currentState != null)
            {
                while (currentTime > 0)
                {
                    currentTime -= Time.deltaTime;
                    float alpha = currentTime / fadeTime;
                    currentState.Menu.SetAlpha(alpha);
                    yield return null;
                }

                currentState.Menu.SetAlpha(0);
            }

            currentState = (MenuState) States.GetState(menuName);
            currentTime = fadeTime;
            
            while (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
                float alpha = 1 - currentTime / fadeTime;
                currentState.Menu.SetAlpha(alpha);
                yield return null;
            }
            currentState.Menu.SetAlpha(1);

            EventSystem.current.sendNavigationEvents = true;
            States.GetState(menuName).Enter();
        }

        //TODO move me somewhere more suitable
        public void OnApplicationQuit()
        {
//            SaveController.SaveSettings();
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

        public static void ReturnToDefault()
        {
            ShowMenu(_instance.InitialMenu.name);
        }
    }
}