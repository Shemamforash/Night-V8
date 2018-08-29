using System;
using System.Collections;
using Facilitating.Persistence;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Libraries;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.MenuSystem
{
    public class MenuStateMachine : MonoBehaviour
    {
        private static MenuStateMachine _instance;
        private static StateMachine States;
        public Menu InitialMenu;
        public static Action OnTransition;

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
                    t.gameObject.SetActive(false);
                    t.GetComponent<CanvasGroup>().alpha = 0;
                }

                States.AddState(menu);
            }
        }

        public void Start()
        {
            if (InitialMenu != null) ShowMenu(InitialMenu.name);
        }

        public static void ShowMenu(string menuName, Action onTransition = null)
        {
            if (States.GetCurrentState()?.Name == menuName) return;
            OnTransition = onTransition;
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
                currentState.SetActive(false);
            }

            currentState = (MenuState) States.GetState(menuName);
            currentState.SetActive(true);
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
            OnTransition?.Invoke();
        }

        public static void SelectInactiveButton(Button button)
        {
            _instance.StartCoroutine(_instance.WaitAndSelect(button));
        }

        private IEnumerator WaitAndSelect(Button button)
        {
            while (!button.gameObject.activeInHierarchy) yield return null;
            button.Select();
        }

        public static void ReturnToDefault()
        {
            ShowMenu(_instance.InitialMenu.name);
        }

        public static Menu CurrentMenu()
        {
            return ((MenuState)States.GetCurrentState()).Menu;
        }
    }
}