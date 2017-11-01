using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Waiting : CombatState
    {
        public Waiting(CombatStateMachine parentMachine) : base(nameof(Waiting), parentMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();
            if (!Weapon().Cocked) NavigateToState("Cocking");
            if (Weapon().GetRemainingAmmo() != 0) return;
            CombatManager.CombatUi.EmptyMagazine();
            CombatManager.CombatUi.SetMagazineText("NO AMMO");
        }

        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            base.OnInputDown(axis, isHeld, direction);
            switch (axis)
            {
                case InputAxis.CancelCover:
                    NavigateToState("Entering Cover");
                    break;
                case InputAxis.Fire:
                    NavigateToState("Firing");
                    break;
                case InputAxis.Reload:
                    NavigateToState("Reloading");
                    break;
                case InputAxis.Horizontal:
                    NavigateToState(direction > 0 ? "Approaching" : "Retreating");
                    break;
                case InputAxis.Flank:
                    NavigateToState("Flanking");
                    break;
            }
        }

        public override void OnInputUp(InputAxis axis)
        {
        }
    }
}