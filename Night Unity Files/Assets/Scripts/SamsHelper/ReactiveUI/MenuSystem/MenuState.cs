using Game.World;
using SamsHelper.BaseGameFunctionality.StateMachines;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.MenuSystem
{
    public class MenuState : State
    {
        private readonly Menu _menu;
        private Selectable _lastSelectedItem;

        public MenuState(StateMachine stateMachine, string name, Menu menu) : base(stateMachine, name)
        {
            _menu = menu;
            SetGameObject(menu.gameObject);
        }

        public override void Enter()
        {
            base.Enter();
            _menu.gameObject.SetActive(true);
            if (_menu.PauseOnOpen)
            {
                WorldState.Pause();
            }
            if (_menu.PreserveLastSelected && _lastSelectedItem != null)
            {
                _lastSelectedItem.Select();
            }
            else
            {
                if (_menu.DefaultSelectable == null)
                {
                    return;
                }
                _menu.DefaultSelectable.Select();
            }
        }

        public override void Exit()
        {
            _lastSelectedItem = EventSystem.current?.currentSelectedGameObject?.GetComponent<Selectable>();
            if (_menu.PauseOnOpen)
            {
                WorldState.UnPause();
            }
            _menu.gameObject.SetActive(false);
        }
    }
}