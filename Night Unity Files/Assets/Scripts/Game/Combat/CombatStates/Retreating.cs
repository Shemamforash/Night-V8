using System;
using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Retreating : CombatState {
        public Retreating(CombatStateMachine parentMachine, bool isPlayerState) : base("Retreating", parentMachine, isPlayerState)
        {
        }

        public override void OnInputUp(InputAxis inputAxis)
        {
            if (inputAxis == InputAxis.Vertical)
            {
                ParentMachine.ReturnToDefault();
            }
        }
    }
}
