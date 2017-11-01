using SamsHelper.Input;

namespace Game.Combat.CombatStates
{
    public class Approaching : CombatState
    {
        public Approaching(CombatStateMachine parentMachine) : base("Approaching", parentMachine)
        {
        }

        public override void Update()
        {
            CombatMachine.Character.DecreaseDistance();
        }

        public override void OnInputUp(InputAxis axis)
        {
            if (axis == InputAxis.Horizontal)
            {
                NavigateToState(nameof(Waiting));
            }
        }
    }
}