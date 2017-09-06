using Game.World;
using Game.World.Time;
using SamsHelper.BaseGameFunctionality.StateMachines;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SamsHelper.ReactiveUI.MenuSystem
{
    public class MenuState : State
    {
        private readonly Menu _menu;
        private Selectable _lastSelectedItem;

        public MenuState(string name, Menu menu, StateMachine parentMachine) : base(name, parentMachine)
        {
            _menu = menu;
        }

        public override void Enter()
        {
            _menu.gameObject.SetActive(true);
            if (_menu.PauseOnOpen)
            {
                WorldTime.Instance().Pause();
            }
            if (_lastSelectedItem != null)
            {
                _lastSelectedItem.Select();
            }
            else if (_menu.Interactable)
            {
                if (_menu.DefaultSelectable == null)
                {
                    throw new Exceptions.DefaultSelectableNotProvidedForMenu(Name());
                }
                _menu.DefaultSelectable.Select();
            }
        }

        public override void Exit()
        {
            _lastSelectedItem = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>();
            if (_menu.PauseOnOpen)
            {
                WorldTime.Instance().UnPause();
            }
            _menu.gameObject.SetActive(false);
        }
    }
}