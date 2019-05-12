using System;
using System.Collections;
using DG.Tweening;
using Extensions;
using Facilitating.UIControllers;
using Game.Combat.Generation;
using Game.Global;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.MenuSystem
{
	public class MenuStateMachine : MonoBehaviour, IInputListener
	{
		private const  float            MenuFadeTime = 0.1f;
		private static MenuStateMachine _instance;
		private static StateMachine     States;
		private static Action           OnTransition;
		private static Sequence         _sequence;
		public         Menu             InitialMenu;

		public void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
		{
			if (isHeld || axis != InputAxis.Menu) return;
			bool isWorldView     = CurrentMenu() is WorldView;
			bool isCombatManager = CurrentMenu() is CombatManager;
			bool isPauseMenuOpen = PauseMenuController.IsOpen();
			bool isGearMenuOpen  = UiGearMenuController.IsOpen();
			bool isTutorialOpen  = TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialVisible();

			if (!isWorldView && !isCombatManager && !isPauseMenuOpen) return;
			if (!isPauseMenuOpen                 && (isGearMenuOpen || isTutorialOpen)) return;
			PauseMenuController.ToggleOpen();
		}

		public void OnInputUp(InputAxis axis)
		{
		}

		public void OnDoubleTap(InputAxis axis, float direction)
		{
		}

		public void Awake()
		{
			if (SceneManager.GetActiveScene().name != "Menu") InputHandler.RegisterInputListener(this);
			States    = new StateMachine();
			_instance = this;
			foreach (Menu t in transform.FindAllComponentsInChildren<Menu>()) RegisterMenu(t);
		}

		private void OnDestroy()
		{
			InputHandler.UnregisterInputListener(this);
		}

		public static void RegisterMenu(Menu t)
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

		public void Start()
		{
			if (InitialMenu != null) ShowMenu(InitialMenu.name);
		}

		public static void ShowMenu(string menuName, Action onTransition = null)
		{
			if (States.GetCurrentState()?.Name == menuName) return;
			ButtonClickListener.SuppressClick();
			OnTransition = onTransition;
			FadeMenu(menuName);
		}

		private static void FadeMenu(string menuName)
		{
			MenuState currentState = (MenuState) States.GetCurrentState();
			MenuState nextState    = (MenuState) States.GetState(menuName);

			_sequence?.Complete(true);
			_sequence                                = DOTween.Sequence().SetUpdate(UpdateType.Normal, true);
			EventSystem.current.sendNavigationEvents = false;
			if (currentState != null) _sequence.Append(DOTween.To(currentState.Menu.GetAlpha, currentState.Menu.SetAlpha, 0f, MenuFadeTime));
			_sequence.AppendCallback(() =>
			{
				currentState?.SetActive(false);
				nextState.SetActive(true);
				nextState.Menu.PreEnter();
			});
			_sequence.Append(DOTween.To(nextState.Menu.GetAlpha, nextState.Menu.SetAlpha, 1f, MenuFadeTime));
			_sequence.AppendCallback(() =>
			{
				EventSystem.current.sendNavigationEvents = true;

				nextState.Enter();
				OnTransition?.Invoke();
			});
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

		public static Menu CurrentMenu() => ((MenuState) States?.GetCurrentState())?.Menu;
	}
}