using Game.Global;
using SamsHelper.BaseGameFunctionality.StateMachines;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SamsHelper.ReactiveUI.MenuSystem
{
    public class MenuState : State
    {
        public readonly Menu Menu;

        public MenuState(StateMachine stateMachine, string name, Menu menu) : base(stateMachine, name)
        {
            Menu = menu;
        }

        public void SetActive(bool active)
        {
            Menu.gameObject.SetActive(active);
        }

        public override void Enter()
        {
            base.Enter();
            Menu.Enter();
            if (Menu.PauseOnOpen) WorldState.Pause();
            if (Menu.DefaultSelectable == null) return;
            GameObject current = EventSystem.current.currentSelectedGameObject;
            if (current != null && current.activeInHierarchy) return;
            Menu.DefaultSelectable.Select();
        }

        public override void Exit()
        {
            if (Menu.PauseOnOpen) WorldState.Resume();
            if (Menu == null) return;
            Menu.Exit();
        }
    }
}