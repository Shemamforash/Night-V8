using Game.Global;
using SamsHelper.BaseGameFunctionality.StateMachines;

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
            Menu.DefaultSelectable.Select();
        }

        public override void Exit()
        {
            if (Menu.PauseOnOpen) WorldState.UnPause();
            if (Menu == null) return;
            Menu.Exit();
        }
    }
}