using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Approaching : CombatState
    {
        public Approaching(CombatStateMachine parentMachine) : base(nameof(Approaching), parentMachine)
        {
        }

        public override void Enter()
        {
            CombatManager.LeaveCover(Character());
        }

        public override void Update()
        {
            CombatMachine.Character.DecreaseDistance();
        }

        public override void OnInputDown(InputAxis axis, bool isHeld, float direction = 0)
        {
            base.OnInputDown(axis, isHeld, direction);
            if (axis != InputAxis.Sprint) return;
            if (CombatManager.DecreaseRage())
            {
                Character().StartSprinting();
            }
            else
            {
                Character().StopSprinting();
            }
        }

        public override void OnInputUp(InputAxis axis)
        {
            base.OnInputUp(axis);
            if (axis == InputAxis.Horizontal) NavigateToState(nameof(Waiting));
        }
    }
}