using SamsHelper.Input;
using UnityEngine;

namespace Game.Combat.CombatStates
{
    public class Aiming : CombatState
    {
        public Aiming(CombatStateMachine parentMachine) : base("Aiming", parentMachine)
        {
            OnUpdate += IncreaseAim;
        }

        public override void Enter()
        {
            Debug.Log("aiming");
            if (Weapon().GetRemainingAmmo() != 0) return;
            CombatManager.CombatUi.EmptyMagazine();
            CombatManager.CombatUi.SetMagazineText("NO AMMO");
        }

        private void IncreaseAim()
        {
            CombatMachine.IncreaseAim();
        }

        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            switch (axis)
            {
                case InputAxis.Cancel:
                    ParentMachine.NavigateToState("Cocking");
                    break;
                case InputAxis.Fire:
                    ParentMachine.NavigateToState("Firing");
                    break;
                case InputAxis.Reload:
                    ParentMachine.NavigateToState("Reloading");
                    break;
                case InputAxis.Vertical:
                    ParentMachine.NavigateToState(direction > 0 ? "Approaching" : "Retreating");
                    break;
                case InputAxis.Horizontal:
                    ParentMachine.NavigateToState(direction > 0 ? "Flanking" : "Entering Cover");
                    break;
            }
        }

        public override void OnInputUp(InputAxis axis)
        {
        }
    }
}