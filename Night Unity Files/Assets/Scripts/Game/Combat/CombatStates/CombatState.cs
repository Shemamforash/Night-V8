using Game.Gear.Weapons;
using SamsHelper.BaseGameFunctionality.Characters;
using SamsHelper.BaseGameFunctionality.StateMachines;
using SamsHelper.Input;
using UnityEngine;
using Character = SamsHelper.BaseGameFunctionality.Characters.Character;

namespace Game.Combat.CombatStates
{
    public abstract class CombatState : State
    {
        protected readonly bool IsPlayerState;
        protected CombatStateMachine CombatMachine;

        protected CombatState(string name, CombatStateMachine combatMachine, bool isPlayerState, bool returnToDefaultOnNoPress = true) : base(name, StateSubtype.Combat, combatMachine)
        {
            CombatMachine = combatMachine;
            IsPlayerState = isPlayerState;
            BindInput(returnToDefaultOnNoPress);
        }

        private void BindInput(bool returnToDefaultOnNoPress)
        {
            if (!IsPlayerState) return;
            InputHandler instance = InputHandler.Instance();
            instance.AddOnPositivePressEvent(InputAxis.Vertical, () => ParentMachine.NavigateToState("Approaching"));
            instance.AddOnNegativePressEvent(InputAxis.Vertical, () => ParentMachine.NavigateToState("Retreating"));
            instance.AddOnPressEvent(InputAxis.Fire, () => ParentMachine.NavigateToState("Firing"));
            instance.AddOnPressEvent(InputAxis.Reload, () => ParentMachine.NavigateToState("Reloading"));
            if (returnToDefaultOnNoPress)
            {
                instance.AddOnNoPress(() => ParentMachine.NavigateToState("Aiming"));
            }
        }

        protected Character Character()
        {
            return CombatMachine.Character;
        }

        protected Weapon Weapon()
        {
            return (Weapon)Character().EquippedGear[GearSubtype.Weapon];
        }
    }
}